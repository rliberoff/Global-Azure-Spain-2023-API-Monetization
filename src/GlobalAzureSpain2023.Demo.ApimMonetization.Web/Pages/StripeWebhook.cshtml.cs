using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

using Stripe;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class StripeWebhookModel : PageModel
    {
        private readonly StripeOptions stripeOptions;

        private readonly IApimService apimService;
        private readonly ILogger<StripeWebhookModel> logger;

        public StripeWebhookModel(IApimService apimService, ILogger<StripeWebhookModel> logger, IOptions<StripeOptions> stripeOptions)
        {
            this.apimService = apimService;
            this.logger = logger;
            this.stripeOptions = stripeOptions.Value;
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var signatureHeader = Request.Headers[@"Stripe-Signature"];

                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, stripeOptions.WebhookSecret);

                Subscription stripeSubscription;

                switch (stripeEvent.Type.ToUpperInvariant())
                {
                    case @"CUSTOMER.SUBSCRIPTION.CREATED":
                        // Create a new APIM subscription for the user when checkout is successful
                        stripeSubscription = stripeEvent.Data.Object as Subscription;

                        var apimProductId = stripeSubscription.Metadata[Constants.ApimProductIdKey];
                        var apimUserId = stripeSubscription.Metadata[Constants.ApimUserIdKey];
                        var apimSubscriptionName = stripeSubscription.Metadata[Constants.ApimSubscriptionNameKey];

                        var createSubscriptionResult = await apimService.CreateSubscriptionAsync(stripeSubscription.Id, apimSubscriptionName, apimUserId, apimProductId, cancellationToken);

                        // Add APIM subscription ID to Stripe subscription metadata
                        await new SubscriptionService().UpdateAsync(stripeSubscription.Id, new SubscriptionUpdateOptions()
                        { 
                            Metadata = new Dictionary<string, string>()
                            {
                                { Constants.ApimSubscriptionIdKey, createSubscriptionResult.Name },
                            }
                        }, cancellationToken: cancellationToken);

                        break;

                    case @"CUSTOMER.SUBSCRIPTION.UPDATED":
                    case @"CUSTOMER.SUBSCRIPTION.DELETED":
                        // Deactivate APIM subscription if Stripe subscription is updated / deleted (as payment may no longer be valid)
                        stripeSubscription = stripeEvent.Data.Object as Subscription;

                        var stripeSubscriptionStatus = stripeSubscription.Status.ToUpperInvariant();

                        Func<string, CancellationToken, Task> subscriptionUpdateMethod = null;

                        switch (stripeSubscriptionStatus)
                        {
                            case @"CANCELED":
                                subscriptionUpdateMethod = apimService.CancelSubscriptionAsync;
                                break;

                            case @"UNPAID":
                                subscriptionUpdateMethod = apimService.SuspendSubscriptionAsync; 
                                break;
                        }

                        if (subscriptionUpdateMethod != null && (stripeSubscriptionStatus == @"CANCELED" || stripeSubscriptionStatus == @"UNPAID"))
                        {
                            if (stripeSubscription.Metadata.TryGetValue(Constants.ApimSubscriptionIdKey, out var apimSubscriptionId) && !string.IsNullOrWhiteSpace(apimSubscriptionId))
                            {

                                var subscription = await apimService.GetSubscriptionAsync(apimSubscriptionId, cancellationToken);
                                var subscriptionState = subscription.StateName.ToUpperInvariant();

                                if (subscriptionState == @"ACTIVE" || subscriptionState == @"SUBMITTED")
                                {
                                    await subscriptionUpdateMethod(apimSubscriptionId, cancellationToken);
                                }
                            }                            
                        }

                        break;

                    default:
                        logger.LogWarning(@"Unhandled event: {Type}", stripeEvent.Type);
                        break;
                }
            }
            catch (StripeException se)
            {
                logger.LogError(@"Error: {Message}", se.Message);
                return BadRequest(se.Message);
            }
            catch (Exception e)
            {
                logger.LogError(@"Error: {Message}", e.Message);
                return StatusCode(500, e.Message);
            }

            // Return a response to acknowledge receipt of the event
            return new JsonResult(new { Received = true });
        }
    }
}

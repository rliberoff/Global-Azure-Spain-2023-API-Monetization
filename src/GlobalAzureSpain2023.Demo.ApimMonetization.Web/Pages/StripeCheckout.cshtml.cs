using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

using Stripe;
using Stripe.Checkout;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class CheckoutStripeModel : PageModel
    {
        private readonly IApimService apimService;
        private readonly IMonetizationService monetizationService;


        private readonly ILogger<CheckoutStripeModel> logger;

        public CheckoutStripeModel(IApimService apimService, IMonetizationService monetizationService, ILogger<CheckoutStripeModel> logger, IOptions<StripeOptions> stripeOptions)
        {
            this.apimService = apimService;
            this.monetizationService = monetizationService;

            this.logger = logger;

            StripeOptions = stripeOptions.Value;
        }

        [BindProperty]
        public StripeOptions StripeOptions { get; }

        [BindProperty]
        public StripeCheckoutRequest StripeCheckoutRequest { get; set; }
        
        public void OnGet()
        {
            StripeCheckoutRequest = new()
            {
                ApimProductId = Request.Query[@"ProductId"],
                ApimSubscriptionName = Request.Query[@"SubscriptionName"],
                ApimUserId = Request.Query[@"UserId"],
                Operation = Request.Query[@"Operation"],
                Salt = Request.Query[@"Salt"],
                Sig = Request.Query[@"Sig"],
                UserEmail = Request.Query[@"UserEmail"],
            };
        }

        public async Task<IActionResult> OnPostAsync([FromBody] StripeCheckoutRequest stripeCheckoutRequest, CancellationToken cancellationToken)
        {
            string productId = stripeCheckoutRequest.ApimProductId;
            string userId = stripeCheckoutRequest.ApimUserId;
            string subscriptionName = stripeCheckoutRequest.ApimSubscriptionName;
            string userEmail = stripeCheckoutRequest.UserEmail;

            var product = await apimService.GetProductAsync(productId, cancellationToken);
            var monetizationModel = await monetizationService.GetMonetizationModelFromProductAsync(product.ProductId, cancellationToken);
            var pricingModelType = monetizationModel.PricingModelType;

            var cancelUriBuilder = new UriBuilder(stripeCheckoutRequest.ReturnUrl)
            {
                Path = @"cancel",
                Query = new QueryString().Add(@"operation", Request.Query[@"Operation"])
                                         .Add(@"userId", userId)
                                         .Add(@"productId", productId)
                                         .Add(@"subscriptionName", subscriptionName)
                                         .Add(@"salt", stripeCheckoutRequest.Salt)
                                         .Add(@"sig", stripeCheckoutRequest.Sig)
                                         .Add(@"userEmail", userEmail)
                                         .Add(@"returnUrl", stripeCheckoutRequest.ReturnUrl)
                                         .ToUriComponent()
            };

            var successUriBuilder = new UriBuilder(stripeCheckoutRequest.ReturnUrl)
            {
                Path = @"success",
                Query = new QueryString().Add(@"userId", userId)
                                         .Add(@"returnUrl", stripeCheckoutRequest.ReturnUrl)
                                         .ToUriComponent()
            };

            StripeConfiguration.ApiKey = StripeOptions.ApplicationKey;

            var sessionCreateOptions = new SessionCreateOptions
            {
                CancelUrl = cancelUriBuilder.Uri.AbsoluteUri,
                SuccessUrl = successUriBuilder.Uri.AbsoluteUri,
                PaymentMethodTypes = new List<string>() { @"card" },
                Mode = @"subscription",
                CustomerEmail = userEmail,
                SubscriptionData = new SessionSubscriptionDataOptions()
                {
                    Metadata = new()
                        {
                            { Constants.ApimUserIdKey,  userId },
                            { Constants.ApimProductIdKey, productId },
                            { Constants.ApimSubscriptionNameKey, subscriptionName },
                        }
                },
                LineItems = new List<SessionLineItemOptions>(),
            };

            var price = (await new PriceService().ListAsync(new PriceListOptions() { Product = product.ProductId, Active = true }, cancellationToken: cancellationToken)).First();

            switch (pricingModelType.ToUpperInvariant())
            {
                case @"TIER":
                    sessionCreateOptions.LineItems.Add(new SessionLineItemOptions()
                    {
                        Price = price.Id,
                        Quantity = 1,
                    });
                    break;

                default:
                    sessionCreateOptions.LineItems.Add(new SessionLineItemOptions()
                    {
                        Price = price.Id,
                    });
                    break;
            }

            var session = await new SessionService().CreateAsync(sessionCreateOptions, cancellationToken: cancellationToken);

            return new JsonResult(new { session.Id });
        }
    }
}

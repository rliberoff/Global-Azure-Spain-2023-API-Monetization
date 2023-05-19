using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

using Stripe;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Pages
{
    public class UnsubscribeModel : PageModel
    {
        private readonly IApimService apimService;
        private readonly ILogger<UnsubscribeModel> logger;

        private readonly ApimServiceOptions apimOptions;
        private readonly StripeOptions stripeOptions;

        public UnsubscribeModel(IApimService apimService, ILogger<UnsubscribeModel> logger, IOptions<ApimServiceOptions> apimOptions, IOptions<StripeOptions> stripeOptions)
        {
            this.apimService = apimService;
            this.logger = logger;

            this.apimOptions = apimOptions.Value;
            this.stripeOptions = stripeOptions.Value;
        }

        [BindProperty]
        public Uri DeveloperPortalUrl { get; private set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                string subscriptionId = Request.Query[@"subscriptionId"];

                await apimService.CancelSubscriptionAsync(subscriptionId, cancellationToken);

                StripeConfiguration.ApiKey = stripeOptions.ApplicationKey;

                await new SubscriptionService().CancelAsync(subscriptionId, cancellationToken: cancellationToken);

                DeveloperPortalUrl = apimOptions.DeveloperPortalUrl;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}

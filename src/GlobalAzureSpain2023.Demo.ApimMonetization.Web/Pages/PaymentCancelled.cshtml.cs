using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Mvc;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Pages
{
    public class PaymentCancelledModel : PageModel
    {
        private readonly ILogger<PaymentCancelledModel> logger;

        public PaymentCancelledModel(ILogger<PaymentCancelledModel> logger)
        {
            this.logger = logger;
        }

        [BindProperty]
        public Uri CheckoutUrl { get; private set; }

        public void OnGet()
        {
            var checkoutUriBuilder = new UriBuilder(Request.Query[@"returnUrl"])
            {
                Path = @"checkout/session",
                Query = new QueryString().Add(@"operation", Request.Query[@"operation"])
                                         .Add(@"userId", Request.Query[@"userId"])
                                         .Add(@"productId", Request.Query[@"productId"])
                                         .Add(@"subscriptionName", Request.Query[@"subscriptionName"])
                                         .Add(@"salt", Request.Query[@"salt"])
                                         .Add(@"sig", Request.Query[@"sig"])
                                         .Add(@"userEmail", Request.Query[@"userEmail"])
                                         .ToUriComponent()
            };

            CheckoutUrl = checkoutUriBuilder.Uri;
        }
    }
}

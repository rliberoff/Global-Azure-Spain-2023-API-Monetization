using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Pages
{
    public class PaymentSucceededModel : PageModel
    {
        private readonly IApimService apimService;
        private readonly ILogger<PaymentSucceededModel> logger;

        public PaymentSucceededModel(IApimService apimService, ILogger<PaymentSucceededModel> logger)
        {
            this.apimService = apimService;
            this.logger = logger;
        }
        [BindProperty]
        public string DeveloperPortalUrl { get; private set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            var sharedAccessToken = await apimService.GetSharedAccessTokenAsync(Request.Query[@"userId"], cancellationToken);

            DeveloperPortalUrl = apimService.BuildDeveloperPortalSignInUrl(sharedAccessToken, Request.Query[@"returnUrl"]);
        }
    }
}

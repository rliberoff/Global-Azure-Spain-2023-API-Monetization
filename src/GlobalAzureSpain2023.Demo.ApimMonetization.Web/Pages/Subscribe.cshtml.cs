using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Pages
{
    public class SubscribeModel : PageModel
    {
        private readonly IApimService apimService;
        private readonly ILogger<SubscribeModel> logger;

        public SubscribeModel(IApimService apimService, ILogger<SubscribeModel> logger)
        {
            this.apimService = apimService;
            this.logger = logger;
        }

        public string ProductDisplayName { get; set; }

        [BindProperty]
        public SubscriptionRequest SubscriptionRequest { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            SubscriptionRequest = new()
            {
                Operation = Request.Query[@"operation"],
                ProductId = Request.Query[@"productId"],
                Salt = Request.Query[@"salt"],
                Sig = Request.Query[@"sig"],
                UserId = Request.Query[@"userId"]
            };

            var product = await apimService.GetProductAsync(SubscriptionRequest.ProductId, cancellationToken);

            ProductDisplayName = product.ProductDisplayName;
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!Utils.ValidateSubscribeRequest(apimService, SubscriptionRequest))
            { 
                return Unauthorized();
            }

            var user = await apimService.GetUserAsync(SubscriptionRequest.UserId, cancellationToken);

            var routeValues = new
            {
                user.UserEmail,
                SubscriptionRequest.UserId,
                SubscriptionRequest.ProductId,
                SubscriptionRequest.SubscriptionName,
                SubscriptionRequest.Operation,
                SubscriptionRequest.Salt,
                SubscriptionRequest.Sig,
            };

            return RedirectToPage(@"StripeCheckout", routeValues);            
        }
    }
}

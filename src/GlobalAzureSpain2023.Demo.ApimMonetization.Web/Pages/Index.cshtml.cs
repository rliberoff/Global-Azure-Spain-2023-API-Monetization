using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Pages;
public class IndexModel : PageModel
{
    private readonly IApimService apimService;
    private readonly ILogger<IndexModel> logger;

    public IndexModel(IApimService apimService, ILogger<IndexModel> logger)
    {
        this.apimService = apimService;
        this.logger = logger;
    }

    public IActionResult OnGet()
    {
        string operation = Request.Query[@"operation"];
        var oper = operation?.ToUpperInvariant();

        switch (oper)
        {
            case @"SUBSCRIBE":
                var subscriptionRequest = new SubscriptionRequest()
                {
                    Operation = operation,
                    ProductId = Request.Query[@"productId"],
                    Salt = Request.Query[@"salt"],
                    Sig = Request.Query[@"sig"],
                    UserId = Request.Query[@"userId"]
                };

                if (!Utils.ValidateSubscribeRequest(apimService, subscriptionRequest))
                {
                    return Unauthorized();
                }

                return RedirectToPage(@"Subscribe", subscriptionRequest);

            case @"UNSUBSCRIBE":
                var unsubscriptionRequest = new SubscriptionRequest()
                {
                    Operation = operation,
                    SubscriptionId = Request.Query[@"subscriptionId"],
                    Salt = Request.Query[@"salt"],
                    Sig = Request.Query[@"sig"],
                };

                if (!Utils.ValidateUnsubscribeRequest(apimService, unsubscriptionRequest))
                {
                    return Unauthorized();
                }

                return RedirectToPage(@"Unsubscribe", unsubscriptionRequest);

            default:
                return BadRequest();
        }
    }
}

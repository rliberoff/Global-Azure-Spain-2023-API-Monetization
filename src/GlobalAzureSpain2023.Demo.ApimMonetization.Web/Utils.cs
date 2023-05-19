using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web;

internal static class Utils
{
    internal static bool ValidateSubscribeRequest(IApimService apimService, SubscriptionRequest request)
    {
        return apimService.ValidateRequest(new[] { request.Salt, request.ProductId, request.UserId }, request.Sig);
    }

    internal static bool ValidateUnsubscribeRequest(IApimService apimService, SubscriptionRequest request)
    {
        return apimService.ValidateRequest(new[] { request.Salt, request.SubscriptionId }, request.Sig);
    }
}


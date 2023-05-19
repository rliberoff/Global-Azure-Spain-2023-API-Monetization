namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

public class StripeCheckoutRequest
{
    public string UserEmail { get; init; }

    public string ApimUserId { get; init; }

    public string ApimProductId { get; init; }

    public string ApimSubscriptionName { get; init; }

    public string Operation { get; init; }

    public string Salt { get; init; }

    public string Sig { get; init; }

    public string ReturnUrl { get; init; }
}

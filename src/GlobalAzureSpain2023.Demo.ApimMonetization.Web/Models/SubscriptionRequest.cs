namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

public class SubscriptionRequest
{
    public string ProductId { get; init; }

    public string Operation { get; init; }

    public string Salt { get; init; }

    public string Sig { get; init; }

    public string UserId { get; init; }

    public string SubscriptionId { get; init; }

    public string SubscriptionName { get; init; }
}

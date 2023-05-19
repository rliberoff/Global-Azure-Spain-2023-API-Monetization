namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;

public sealed class ApimServiceOptions
{
    public string DelegationValidationKey { get; init; }

    public Uri DeveloperPortalUrl { get; init; }

    public Uri ManagementUrl { get; init; }

    public string ServiceName { get; init; }

    public string ResourceGroupName { get; init; }

    public string SubscriptionId { get; init; }
}

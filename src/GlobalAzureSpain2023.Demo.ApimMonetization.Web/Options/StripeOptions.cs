namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;

public sealed class StripeOptions
{
    public string ApplicationKey { get; init; }

    public string PublishableKey { get; init; }

    public string WebhookSecret { get; init; }
}

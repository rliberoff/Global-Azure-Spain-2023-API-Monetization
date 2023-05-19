using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;

public interface IApimService
{
    string BuildDeveloperPortalSignInUrl(string sharedAccessToken, string returnUrl);

    Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);

    Task<CreateSubscriptionResult> CreateSubscriptionAsync(string subscriptionId, string subscriptionName, string userId, string productId, CancellationToken cancellationToken);

    Task<GetProductResult> GetProductAsync(string productId, CancellationToken cancellationToken);

    Task<GetUserResult> GetUserAsync(string userId, CancellationToken cancellationToken);

    Task<string> GetSharedAccessTokenAsync(string userId, CancellationToken cancellationToken);

    Task<GetSubscriptionResult> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);

    Task SuspendSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);

    bool ValidateRequest(IEnumerable<string> parameters, string expectedSignature);
}

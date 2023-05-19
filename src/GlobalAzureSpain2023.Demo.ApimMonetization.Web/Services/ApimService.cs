using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text;

using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager.ApiManagement.Models;

using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;

using Microsoft.Extensions.Options;
using Azure.Core;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Services;

internal sealed class ApimService : IApimService
{
    private readonly ApimServiceOptions options;

    private readonly ArmClient client;

    public ApimService(IOptions<ApimServiceOptions> options)
    {
        this.options = options.Value;

        client = new ArmClient(new DefaultAzureCredential());

    }

    public string BuildDeveloperPortalSignInUrl(string sharedAccessToken, string returnUrl)
    {
        var queryString = new QueryString();
        queryString = queryString.Add(@"token", sharedAccessToken);
        queryString = queryString.Add(@"returnUrl", returnUrl);

        var uriBuilder = new UriBuilder(options.DeveloperPortalUrl)
        {
            Path = @"signin-sso",
            Query = queryString.ToUriComponent()
        };

        return uriBuilder.Uri.AbsoluteUri;
    }

    public Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        return UpdateSubscriptionStateAsync(subscriptionId, SubscriptionState.Cancelled, cancellationToken);
    }

    public async Task<CreateSubscriptionResult> CreateSubscriptionAsync(string subscriptionId, string subscriptionName, string userId, string productId, CancellationToken cancellationToken)
    {
        // Creamos el identificador de un Azure API Management.
        var apiManagementServiceResourceId = ApiManagementServiceResource.CreateResourceIdentifier(options.SubscriptionId, options.ResourceGroupName, options.ServiceName);
        
        // Obtenemos la instancia del Azure API Management a partir del ArmClient.
        // Esta llamada nos retorna una instancia de "ApiManagementServiceResource" que nos ofrece todos los
        // métodos para manipilar elementos de un Azure API Management (APIM).
        var apiManagementService = client.GetApiManagementServiceResource(apiManagementServiceResourceId);
        
        // Por ejemplo, obtener la colección de subscripciones que existen en el APIM
        var subscriptionsCollection = apiManagementService.GetApiManagementSubscriptions();

        var content = new ApiManagementSubscriptionCreateOrUpdateContent()
        {
            DisplayName = subscriptionName,
            OwnerId = $@"/users/{userId}",
            Scope = $@"/products/{productId}",
            State = SubscriptionState.Active,
        };

        var result = await subscriptionsCollection.CreateOrUpdateAsync(WaitUntil.Completed, subscriptionId, content, cancellationToken: cancellationToken);

        return new CreateSubscriptionResult()
        {
            Name = result.Value.Data.Name,
        };
    }

    public async Task<GetProductResult> GetProductAsync(string productId, CancellationToken cancellationToken)
    {
        ResourceIdentifier apiManagementProductResourceId = ApiManagementProductResource.CreateResourceIdentifier(options.SubscriptionId, options.ResourceGroupName, options.ServiceName, productId);
        ApiManagementProductResource apiManagementProduct = client.GetApiManagementProductResource(apiManagementProductResourceId);

        var result = await apiManagementProduct.GetAsync(cancellationToken);

        var resourceData = result.Value.Data;

        return new GetProductResult()
        {
            ProductId = productId,
            ProductDisplayName = resourceData.DisplayName,
            ProductName = resourceData.Name,
        };
    }

    public async Task<string> GetSharedAccessTokenAsync(string userId, CancellationToken cancellationToken)
    {
        var content = new UserTokenContent()
        {
            KeyType = TokenGenerationUsedKeyType.Primary,
            ExpireOn = DateTimeOffset.UtcNow.AddDays(1)
        };

        var apiManagementUserResourceId = ApiManagementUserResource.CreateResourceIdentifier(options.SubscriptionId, options.ResourceGroupName, options.ServiceName, userId);
        var apiManagementUser = client.GetApiManagementUserResource(apiManagementUserResourceId);
        var result = await apiManagementUser.GetSharedAccessTokenAsync(content, cancellationToken);
        return result.Value.Value;
    }

    public async Task<GetSubscriptionResult> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        var apiManagementServiceResourceId = ApiManagementServiceResource.CreateResourceIdentifier(options.SubscriptionId, options.ResourceGroupName, options.ServiceName);
        var apiManagementService = client.GetApiManagementServiceResource(apiManagementServiceResourceId);
        var subscriptionsCollection = apiManagementService.GetApiManagementSubscriptions();

        var result = await subscriptionsCollection.GetAsync(subscriptionId, cancellationToken);

        return new GetSubscriptionResult()
        {
            StateName = result.Value.Data.State.ToString(),
        };
    }

    public async Task<GetUserResult> GetUserAsync(string userId, CancellationToken cancellationToken)
    {
        var apiManagementUserResourceId = ApiManagementUserResource.CreateResourceIdentifier(options.SubscriptionId, options.ResourceGroupName, options.ServiceName, userId);
        var apiManagementUser = client.GetApiManagementUserResource(apiManagementUserResourceId);

        var result = await apiManagementUser.GetAsync(cancellationToken);

        var resourceData = result.Value.Data;

        return new GetUserResult()
        {
            UserEmail = resourceData.Email,
            UserId = resourceData.Id,
        };
    }

    public Task SuspendSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        return UpdateSubscriptionStateAsync(subscriptionId, SubscriptionState.Suspended, cancellationToken);
    }

    public bool ValidateRequest(IEnumerable<string> parameters, string expectedSignature)
    {
        using var encoder = new HMACSHA512(Convert.FromBase64String(options.DelegationValidationKey));

        return expectedSignature == Convert.ToBase64String(encoder.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", parameters))));
    }

    private async Task UpdateSubscriptionStateAsync(string subscriptionId, SubscriptionState state, CancellationToken cancellationToken)
    {
        var apiManagementServiceResourceId = ApiManagementServiceResource.CreateResourceIdentifier(options.SubscriptionId, options.ResourceGroupName, options.ServiceName);
        var apiManagementService = client.GetApiManagementServiceResource(apiManagementServiceResourceId);
        var subscriptionsCollection = apiManagementService.GetApiManagementSubscriptions();

        var content = new ApiManagementSubscriptionCreateOrUpdateContent()
        {
            Scope = @"/apis",
            State = state,
        };

        await subscriptionsCollection.CreateOrUpdateAsync(WaitUntil.Completed, subscriptionId, content, cancellationToken: cancellationToken);
    }
}

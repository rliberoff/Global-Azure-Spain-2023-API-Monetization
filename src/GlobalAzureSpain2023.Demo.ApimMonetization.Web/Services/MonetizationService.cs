using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;
using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Options;

using Microsoft.Extensions.Options;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Services;

internal sealed class MonetizationService : IMonetizationService
{
    public readonly IHttpClientFactory httpClientFactory;
    public readonly MonetizationOptions options;

    public MonetizationService(IHttpClientFactory httpClientFactory, IOptions<MonetizationOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
    }

    public async Task<Monetization> GetMonetizationModelFromProductAsync(string productId, CancellationToken cancellationToken)
    {
        var monetizations = await GetMonetizationModelAsync(cancellationToken: cancellationToken);

        return monetizations.Single(item => item.Id == productId);
    }

    public async Task<IEnumerable<Monetization>> GetMonetizationModelAsync(CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();

        using var response = await httpClient.GetAsync(options.ModelUrl, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IEnumerable<Monetization>>(cancellationToken: cancellationToken);
    }
}

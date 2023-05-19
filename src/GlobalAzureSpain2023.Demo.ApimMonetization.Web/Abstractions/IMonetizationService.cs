using GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Abstractions;

public interface IMonetizationService
{
    Task<IEnumerable<Monetization>> GetMonetizationModelAsync(CancellationToken cancellationToken);

    Task<Monetization> GetMonetizationModelFromProductAsync(string productId, CancellationToken cancellationToken);
}

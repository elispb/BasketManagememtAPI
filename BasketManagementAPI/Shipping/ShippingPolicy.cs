using BasketManagementAPI.Domain.Shipping;
using BasketManagementAPI.Repositories;

namespace BasketManagementAPI.Shipping;

public sealed class ShippingPolicy : IShippingPolicy
{
    private const int DefaultShippingCost = 1299;

    private readonly IShippingCostRepository _shippingCostRepository;

    public ShippingPolicy(IShippingCostRepository shippingCostRepository)
    {
        _shippingCostRepository = shippingCostRepository;
    }

    public async Task<ShippingDetails> ResolveAsync(string country)
    {
        var normalized = (country ?? string.Empty).Trim();
        var resolvedCountry = string.IsNullOrWhiteSpace(normalized) ? "Unknown" : normalized;
        var cost = await _shippingCostRepository.GetCostAsync(normalized);

        return new ShippingDetails(resolvedCountry, cost ?? DefaultShippingCost);
    }
}



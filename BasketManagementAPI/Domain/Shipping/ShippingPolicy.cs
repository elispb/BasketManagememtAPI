using BasketManagementAPI.Repositories;

namespace BasketManagementAPI.Domain.Shipping;

public sealed class ShippingPolicy : IShippingPolicy
{
    private const int DefaultShippingCost = 1299;

    private readonly IShippingCostRepository _shippingCostRepository;

    public ShippingPolicy(IShippingCostRepository shippingCostRepository)
    {
        _shippingCostRepository = shippingCostRepository;
    }

    public async Task<ShippingDetails> ResolveAsync(string countryCode)
    {
        if (!CountryCodeParser.TryParse(countryCode, out var resolvedCode))
        {
            resolvedCode = CountryCode.Unknown;
        }

        var cost = resolvedCode == CountryCode.Unknown
            ? null
            : await _shippingCostRepository.GetCostAsync(resolvedCode);

        return new ShippingDetails(resolvedCode, cost ?? DefaultShippingCost);
    }
}



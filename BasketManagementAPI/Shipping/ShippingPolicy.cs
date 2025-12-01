using BasketManagementAPI.Domain.Shipping;

namespace BasketManagementAPI.Shipping;

public sealed class ShippingPolicy : IShippingPolicy
{
    private const int UkShippingCost = 499;
    private const int DefaultShippingCost = 1299;

    public ShippingDetails Resolve(string country)
    {
        var normalized = (country ?? string.Empty).Trim();
        var upper = normalized.ToUpperInvariant();

        var cost = upper switch
        {
            "UK" or "UNITED KINGDOM" => UkShippingCost,
            _ => DefaultShippingCost
        };

        return new ShippingDetails(string.IsNullOrWhiteSpace(normalized) ? "Unknown" : normalized, cost);
    }
}



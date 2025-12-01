using BasketManagementAPI.Domain.Shipping;

namespace BasketManagementAPI.Shipping;

public sealed class ShippingPolicy : IShippingPolicy
{
    private const decimal UkShippingCost = 4.99m;
    private const decimal DefaultShippingCost = 12.99m;

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


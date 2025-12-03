using BasketManagementAPI.Domain.Shipping;

namespace BasketManagementAPI.Shipping;

public interface IShippingPolicy
{
    Task<ShippingDetails> ResolveAsync(string countryCode);
}



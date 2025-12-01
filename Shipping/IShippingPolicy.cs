using BasketManagememtAPI.Domain.Shipping;

namespace BasketManagememtAPI.Shipping;

public interface IShippingPolicy
{
    ShippingDetails Resolve(string country);
}


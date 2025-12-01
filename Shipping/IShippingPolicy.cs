using BasketManagementAPI.Domain.Shipping;

namespace BasketManagementAPI.Shipping;

public interface IShippingPolicy
{
    ShippingDetails Resolve(string country);
}



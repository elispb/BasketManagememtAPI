namespace BasketManagementAPI.Domain.Shipping;

public interface IShippingPolicy
{
    Task<ShippingDetails> ResolveAsync(string countryCode);
}



using BasketManagementAPI.Domain.Shipping;

namespace BasketManagementAPI.Repositories;

public interface IShippingCostRepository
{
    Task<int?> GetCostAsync(CountryCode countryCode);
}


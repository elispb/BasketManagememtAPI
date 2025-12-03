namespace BasketManagementAPI.Repositories;

public interface IShippingCostRepository
{
    Task<int?> GetCostAsync(string country);
}


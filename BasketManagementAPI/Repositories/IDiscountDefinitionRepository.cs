using BasketManagementAPI.Domain.Discounts;

namespace BasketManagementAPI.Repositories;

public interface IDiscountDefinitionRepository
{
    Task<DiscountDefinition?> GetByCodeAsync(string code);

    Task<Guid> UpsertAsync(string code, decimal percentage);
}


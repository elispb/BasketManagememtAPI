using BasketManagementAPI.Domain.Discounts;

namespace BasketManagementAPI.Repositories;

public interface IDiscountDefinitionRepository
{
    Task<DiscountDefinition?> GetByIdAsync(Guid id);

    Task<DiscountDefinition?> GetByCodeAsync(string code);

    Task<Guid> UpsertAsync(string code, decimal percentage);
}


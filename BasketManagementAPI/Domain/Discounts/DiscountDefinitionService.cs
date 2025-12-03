using BasketManagementAPI.Repositories;

namespace BasketManagementAPI.Domain.Discounts;

public interface IDiscountDefinitionService
{
    Task<Guid> EnsureDefinitionAsync(string code, decimal percentage);

    Task<int> CalculateDiscountAsync(Guid? definitionId, int eligibleAmount);
}

public sealed class DiscountDefinitionService : IDiscountDefinitionService
{
    private readonly IDiscountDefinitionRepository _repository;

    public DiscountDefinitionService(IDiscountDefinitionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<Guid> EnsureDefinitionAsync(string code, decimal percentage)
    {
        return _repository.UpsertAsync(code, percentage);
    }

    public async Task<int> CalculateDiscountAsync(Guid? definitionId, int eligibleAmount)
    {
        if (!definitionId.HasValue)
        {
            return 0;
        }

        var definition = await _repository.GetByIdAsync(definitionId.Value);
        if (definition is null || !definition.IsActive || definition.Percentage is null || definition.Percentage <= 0)
        {
            return 0;
        }

        var discountEngine = new PercentageBasketDiscount(definition.Code, definition.Percentage.Value);
        var calculated = discountEngine.CalculateDiscount(eligibleAmount);
        return Math.Min(calculated, eligibleAmount);
    }
}


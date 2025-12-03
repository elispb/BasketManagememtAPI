using BasketManagementAPI.Repositories;
using System;

namespace BasketManagementAPI.Domain.Discounts;

public interface IDiscountCatalog
{
    Task<DiscountDefinition> EnsureDefinitionAsync(string code, decimal percentage);

    Task<DiscountDefinition?> GetActiveDefinitionAsync(Guid? definitionId);
}

public sealed class DiscountCatalog : IDiscountCatalog
{
    private readonly IDiscountDefinitionRepository _repository;

    public DiscountCatalog(IDiscountDefinitionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<DiscountDefinition> EnsureDefinitionAsync(string code, decimal percentage)
    {
        var id = await _repository.UpsertAsync(code, percentage);
        var definition = await _repository.GetByIdAsync(id);

        if (definition is null)
        {
            throw new InvalidOperationException($"Discount definition '{code}' could not be resolved.");
        }

        if (!definition.IsActive || definition.Percentage is null || definition.Percentage <= 0)
        {
            throw new InvalidOperationException($"Discount definition '{code}' is not active or invalid.");
        }

        return definition;
    }

    public async Task<DiscountDefinition?> GetActiveDefinitionAsync(Guid? definitionId)
    {
        if (!definitionId.HasValue)
        {
            return null;
        }

        var definition = await _repository.GetByIdAsync(definitionId.Value);
        if (definition is null || !definition.IsActive || definition.Percentage is null || definition.Percentage <= 0)
        {
            return null;
        }

        return definition;
    }
}


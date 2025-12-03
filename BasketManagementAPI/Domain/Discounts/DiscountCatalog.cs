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
        var existing = await _repository.GetByCodeAsync(code);
        if (existing is not null)
        {
            EnsureActiveDefinition(existing);

            var storedPercentage = existing.Percentage!.Value;
            if (storedPercentage != percentage)
            {
                throw new InvalidOperationException($"Discount definition '{code}' already exists with percentage '{storedPercentage}'.");
            }

            return existing;
        }

        var id = await _repository.UpsertAsync(code, percentage);
        var definition = await _repository.GetByIdAsync(id);

        if (definition is null)
        {
            throw new InvalidOperationException($"Discount definition '{code}' could not be resolved.");
        }

        EnsureActiveDefinition(definition);

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

    private static void EnsureActiveDefinition(DiscountDefinition definition)
    {
        if (!definition.IsActive || definition.Percentage is null || definition.Percentage <= 0)
        {
            throw new InvalidOperationException($"Discount definition '{definition.Code}' is not active or invalid.");
        }
    }
}


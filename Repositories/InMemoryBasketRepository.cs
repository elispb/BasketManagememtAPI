using System.Collections.Concurrent;
using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Repositories;

public sealed class InMemoryBasketRepository : IBasketRepository
{
    private readonly ConcurrentDictionary<Guid, Basket> _store = new();

    public Task<Basket> GetAsync(Guid id)
    {
        if (_store.TryGetValue(id, out var basket))
        {
            return Task.FromResult(basket);
        }

        throw new KeyNotFoundException($"Basket '{id}' not found.");
    }

    public Task CreateAsync(Basket basket)
    {
        if (!_store.TryAdd(basket.Id, basket))
        {
            throw new InvalidOperationException($"Basket '{basket.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task SaveAsync(Basket basket)
    {
        _store[basket.Id] = basket;
        return Task.CompletedTask;
    }
}



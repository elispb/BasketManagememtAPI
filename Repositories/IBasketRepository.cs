using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Repositories;

public interface IBasketRepository
{
    Task<Basket> GetAsync(Guid id);

    Task CreateAsync(Basket basket);

    Task SaveAsync(Basket basket);
}


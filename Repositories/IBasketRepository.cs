using BasketManagememtAPI.Domain.Baskets;

namespace BasketManagememtAPI.Repositories;

public interface IBasketRepository
{
    Task<Basket> GetAsync(Guid id);

    Task CreateAsync(Basket basket);

    Task SaveAsync(Basket basket);
}


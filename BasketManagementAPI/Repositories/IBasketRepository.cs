using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Repositories;

public interface IBasketRepository
{
    Task<Basket> GetAsync(Guid id);

    Task CreateAsync(Basket basket);

    Task SaveAsync(Basket basket);

    Task<bool> DeleteItemAsync(Guid basketId, string productId);

    Task<bool> UpdateItemDiscountAsync(Guid basketId, string productId, byte? itemDiscountType, int? itemDiscountAmount);

    Task<Item?> GetItemAsync(Guid basketId, string productId);
}
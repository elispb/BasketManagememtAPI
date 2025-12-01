using System.Collections.Generic;
using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Services;

public interface IBasketService
{
    Task<Basket> CreateBasketAsync();

    Task<Basket> AddItemsAsync(Guid basketId, IEnumerable<BasketItemDefinition> items);

    Task<Basket> RemoveItemAsync(Guid basketId, string productId);

    Task<Basket> ApplyDiscountCodeAsync(Guid basketId, string code, decimal percentage);

    Task<BasketTotals> AddShippingAsync(Guid basketId, string country);

    Task<BasketTotals> GetTotalsAsync(Guid basketId);

    Task<BasketWithTotals> GetBasketAsync(Guid basketId);

    Task<BasketItem> ApplyItemDiscountAsync(Guid basketId, string productId, ItemDiscountDefinition discount);
}


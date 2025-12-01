using System.Collections.Generic;
using BasketManagememtAPI.Domain.Baskets;

namespace BasketManagememtAPI.Services;

public interface IBasketService
{
    Task<Basket> CreateBasketAsync();

    Task<Basket> AddItemsAsync(Guid basketId, IEnumerable<ItemDefinition> items);

    Task<Basket> RemoveItemAsync(Guid basketId, string productId);

    Task<Basket> ApplyDiscountCodeAsync(Guid basketId, string code, decimal percentage);

    Task<Totals> AddShippingAsync(Guid basketId, string country);

    Task<Totals> GetTotalsAsync(Guid basketId);

    Task<BasketSnapshot> GetBasketAsync(Guid basketId);

    Task<Item> ApplyItemDiscountAsync(Guid basketId, string productId, ItemDiscountDefinition discount);
}


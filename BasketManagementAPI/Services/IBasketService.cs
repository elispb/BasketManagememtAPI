using System.Collections.Generic;
using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Services;

public interface IBasketService
{
    Task<Basket> CreateBasketAsync();

    Task<IReadOnlyCollection<Item>> AddItemsAsync(Guid basketId, IEnumerable<ItemDefinition> items);

    Task RemoveItemAsync(Guid basketId, int productId);

    Task<Basket> ApplyDiscountCodeAsync(Guid basketId, string code, decimal percentage);

    Task<Totals> AddShippingAsync(Guid basketId, string country);

    Task<Totals> GetTotalsAsync(Guid basketId);

    Task<BasketSnapshot> GetBasketAsync(Guid basketId);

    Task<Item> ApplyItemDiscountAsync(Guid basketId, int productId, ItemDiscountDefinition discount);

    Task<Item?> GetItemAsync(Guid basketId, int productId);

    Task<ItemPriceTotals?> GetItemTotalsAsync(Guid basketId, int productId);
}



using System.Collections.Generic;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Repositories;
using BasketManagementAPI.Shipping;

namespace BasketManagementAPI.Services;

public sealed class BasketService : IBasketService
{
    private readonly IBasketRepository _repository;
    private readonly IShippingPolicy _shippingPolicy;
    private readonly ITotalsCalculator _totalsCalculator;
    private readonly IDiscountCatalog _discountCatalog;

    public BasketService(
        IBasketRepository repository,
        IShippingPolicy shippingPolicy,
        ITotalsCalculator totalsCalculator,
        IDiscountCatalog discountCatalog)
    {
        _repository = repository;
        _shippingPolicy = shippingPolicy;
        _totalsCalculator = totalsCalculator;
        _discountCatalog = discountCatalog;
    }

    public async Task<Basket> CreateBasketAsync()
    {
        var basket = new Basket();
        await _repository.CreateAsync(basket);
        return basket;
    }

    public async Task<IReadOnlyCollection<Item>> AddItemsAsync(Guid basketId, IEnumerable<ItemDefinition> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var basket = await _repository.GetAsync(basketId);
        var processedItems = new List<Item>();

        foreach (var itemDefinition in items)
        {
            var discount = itemDefinition.ItemDiscount is null
                ? null
                : ItemDiscountFactory.Create(itemDefinition.ItemDiscount.Type, itemDefinition.ItemDiscount.Amount);

            var item = Item.Create(
                itemDefinition.Name,
                itemDefinition.UnitPrice,
                itemDefinition.Quantity,
                discount);

            var tracked = basket.AddOrUpdateItem(item);
            if (!processedItems.Contains(tracked))
            {
                processedItems.Add(tracked);
            }
        }

        await _repository.SaveAsync(basket);
        return processedItems;
    }

    public async Task RemoveItemAsync(Guid basketId, int productId)
    {
        await _repository.DeleteItemAsync(basketId, productId);
    }

    public async Task<Basket> ApplyDiscountCodeAsync(Guid basketId, string code, decimal percentage)
    {
        var basket = await _repository.GetAsync(basketId);
        var definition = await _discountCatalog.EnsureDefinitionAsync(code, percentage);
        var discount = new PercentageBasketDiscount(definition.Code, definition.Percentage!.Value);
        basket.ApplyDiscount(discount, definition.Id);
        await _repository.SaveAsync(basket);
        return basket;
    }

    public async Task<Totals> AddShippingAsync(Guid basketId, string countryCode)
    {
        var basket = await _repository.GetAsync(basketId);
        var shipping = await _shippingPolicy.ResolveAsync(countryCode);
        basket.SetShipping(shipping);
        await _repository.SaveAsync(basket);
        return await _totalsCalculator.CalculateAsync(basket);
    }

    public async Task<Totals> GetTotalsAsync(Guid basketId)
    {
        var basket = await _repository.GetAsync(basketId);
        return await _totalsCalculator.CalculateAsync(basket);
    }

    public async Task<Item> ApplyItemDiscountAsync(Guid basketId, int productId, ItemDiscountDefinition discount)
    {
        var discountEngine = ItemDiscountFactory.Create(discount.Type, discount.Amount);

        if (discountEngine is null)
        {
            throw new InvalidOperationException("Unable to resolve item discount.");
        }

        var (type, amount) = ItemDiscountFactory.ToPersistedData(discountEngine);
        var item = await _repository.UpdateItemDiscountAsync(basketId, productId, type, amount);

        if (item is null)
        {
            throw new KeyNotFoundException($"Item '{productId}' was not found in basket '{basketId}'.");
        }

        return item;
    }

    public async Task<BasketSnapshot> GetBasketAsync(Guid basketId)
    {
        var basket = await _repository.GetAsync(basketId);
        var totals = await _totalsCalculator.CalculateAsync(basket);
        return new BasketSnapshot(basket, totals);
    }

    public async Task<IReadOnlyCollection<BasketSnapshot>> GetAllBasketsAsync()
    {
        var baskets = await _repository.GetAllAsync();
        var snapshots = new List<BasketSnapshot>(baskets.Count);
        foreach (var basket in baskets)
        {
            var totals = await _totalsCalculator.CalculateAsync(basket);
            snapshots.Add(new BasketSnapshot(basket, totals));
        }

        return snapshots;
    }

    public Task<Item?> GetItemAsync(Guid basketId, int productId)
    {
        return _repository.GetItemAsync(basketId, productId);
    }

    public async Task<ItemPriceTotals?> GetItemTotalsAsync(Guid basketId, int productId)
    {
        var item = await GetItemAsync(basketId, productId);
        if (item is null)
        {
            return null;
        }

        var lineTotal = item.Total();
        var vatAmount = (int)Math.Round(lineTotal * 0.20m, 0, MidpointRounding.AwayFromZero);
        return new ItemPriceTotals(lineTotal, vatAmount, lineTotal + vatAmount);
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using BasketManagememtAPI.Domain.Baskets;
using BasketManagememtAPI.Domain.Discounts;
using BasketManagememtAPI.Repositories;
using BasketManagememtAPI.Shipping;
using BasketManagementAPI.Domain.Discounts;

namespace BasketManagememtAPI.Services;

public sealed class BasketService : IBasketService
{
    private const decimal VatRate = 0.20m;

    private readonly IBasketRepository _repository;
    private readonly IShippingPolicy _shippingPolicy;

    public BasketService(IBasketRepository repository, IShippingPolicy shippingPolicy)
    {
        _repository = repository;
        _shippingPolicy = shippingPolicy;
    }

    public async Task<Basket> CreateBasketAsync()
    {
        var basket = new Basket();
        await _repository.CreateAsync(basket);
        return basket;
    }

    public async Task<Basket> AddItemsAsync(Guid basketId, IEnumerable<ItemDefinition> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var basket = await _repository.GetAsync(basketId);

        foreach (var itemDefinition in items)
        {
            var discount = CreateItemDiscount(itemDefinition.ItemDiscount);
            var item = new Item(
                itemDefinition.ProductId,
                itemDefinition.Name,
                itemDefinition.UnitPrice,
                itemDefinition.Quantity,
                discount);

            basket.AddOrUpdateItem(item);
        }

        await _repository.SaveAsync(basket);
        return basket;
    }

    public async Task<Basket> RemoveItemAsync(Guid basketId, string productId)
    {
        var basket = await _repository.GetAsync(basketId);

        if (!basket.RemoveItem(productId))
        {
            throw new KeyNotFoundException($"Item '{productId}' not found in basket.");
        }

        await _repository.SaveAsync(basket);
        return basket;
    }

    public async Task<Basket> ApplyDiscountCodeAsync(Guid basketId, string code, decimal percentage)
    {
        var basket = await _repository.GetAsync(basketId);
        var discount = new PercentageBasketDiscount(code, percentage);
        basket.ApplyDiscount(discount);
        await _repository.SaveAsync(basket);
        return basket;
    }

    public async Task<Totals> AddShippingAsync(Guid basketId, string country)
    {
        var basket = await _repository.GetAsync(basketId);
        var shipping = _shippingPolicy.Resolve(country);
        basket.SetShipping(shipping);
        await _repository.SaveAsync(basket);
        return BuildTotals(basket);
    }

    public async Task<Totals> GetTotalsAsync(Guid basketId)
    {
        var basket = await _repository.GetAsync(basketId);
        return BuildTotals(basket);
    }

    public async Task<Item> ApplyItemDiscountAsync(Guid basketId, string productId, ItemDiscountDefinition discount)
    {
        var basket = await _repository.GetAsync(basketId);
        var item = basket.Items.FirstOrDefault(i => string.Equals(i.ProductId, productId, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            throw new KeyNotFoundException($"Item '{productId}' was not found in basket '{basketId}'.");
        }

        var discountEngine = CreateItemDiscount(discount);

        if (discountEngine is null)
        {
            throw new InvalidOperationException("Unable to resolve item discount.");
        }

        item.ApplyDiscount(discountEngine);
        await _repository.SaveAsync(basket);
        return item;
    }

    public async Task<BasketSnapshot> GetBasketAsync(Guid basketId)
    {
        var basket = await _repository.GetAsync(basketId);
        return new BasketSnapshot(basket, BuildTotals(basket));
    }

    private static IBasketItemDiscount? CreateItemDiscount(ItemDiscountDefinition? definition)
    {
        if (definition is null)
        {
            return null;
        }

        var trimmedType = definition.Type?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedType))
        {
            throw new ArgumentException("Discount type is required when specifying an item discount.", nameof(definition));
        }

        return trimmedType.ToLowerInvariant() switch
        {
            "flatamount" => new FlatAmountItemDiscount(definition.Amount),
            "bogo" => new BuyOneGetOneFreeItemDiscount(),
            _ => throw new NotSupportedException($"Item discount type '{definition.Type}' is not supported.")
        };
    }

    private static Totals BuildTotals(Basket basket)
    {
        var subtotal = basket.Items.Sum(item => item.Total());
        var eligibleAmount = basket.Items.Where(item => !item.HasItemDiscount).Sum(item => item.Total());
        var discount = basket.BasketDiscount?.CalculateDiscount(eligibleAmount) ?? 0;
        var shipping = basket.ShippingDetails?.Cost ?? 0;
        var totalWithoutVat = Math.Max(subtotal - discount + shipping, 0);
        var vatAmount = (int)Math.Round(totalWithoutVat * VatRate, 0, MidpointRounding.AwayFromZero);
        var totalWithVat = totalWithoutVat + vatAmount;

        return new Totals(subtotal, discount, shipping, totalWithoutVat, vatAmount, totalWithVat);
    }
}


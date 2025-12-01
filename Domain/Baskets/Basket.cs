using System;
using System.Linq;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;

namespace BasketManagementAPI.Domain.Baskets;

public sealed class Basket
{
    public Guid Id { get; }

    public List<BasketItem> Items { get; }

    public ShippingDetails? ShippingDetails { get; private set; }

    public IBasketDiscount? BasketDiscount { get; private set; }

    public Basket()
    {
        Id = Guid.NewGuid();
        Items = new List<BasketItem>();
    }

    public void AddOrUpdateItem(BasketItem item)
    {
        var existing = Items.FirstOrDefault(i => i.Matches(item));

        if (existing is null)
        {
            Items.Add(item);
            return;
        }

        existing.IncreaseQuantity(item.Quantity);
    }

    public bool RemoveItem(string productId)
    {
        var item = Items.FirstOrDefault(i => string.Equals(i.ProductId, productId, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            return false;
        }

        return Items.Remove(item);
    }

    public void ApplyDiscount(IBasketDiscount discount)
    {
        BasketDiscount = discount;
    }

    public void SetShipping(ShippingDetails shipping)
    {
        ShippingDetails = shipping;
    }
}


using System;
using System.Linq;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;

namespace BasketManagementAPI.Domain.Baskets;

public sealed class Basket
{
    public Guid Id { get; }

    public List<Item> Items { get; }

    public ShippingDetails? ShippingDetails { get; private set; }

    public IBasketDiscount? BasketDiscount { get; private set; }

    public Guid? DiscountDefinitionId { get; private set; }

    public Basket() : this(Guid.NewGuid())
    {
    }

    public Basket(Guid id)
    {
        Id = id;
        Items = new List<Item>();
    }

    public Item AddOrUpdateItem(Item item)
    {
        var existing = Items.FirstOrDefault(i => i.Matches(item));

        if (existing is null)
        {
            Items.Add(item);
            return item;
        }

        existing.IncreaseQuantity(item.Quantity);
        return existing;
    }

    public bool RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
        {
            return false;
        }

        return Items.Remove(item);
    }

    public void ApplyDiscount(IBasketDiscount discount, Guid? discountDefinitionId = null)
    {
        BasketDiscount = discount;
        DiscountDefinitionId = discountDefinitionId;
    }

    public void SetShipping(ShippingDetails shipping)
    {
        ShippingDetails = shipping;
    }
}


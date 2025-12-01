using System;
using BasketManagementAPI.Domain.Discounts;

namespace BasketManagementAPI.Domain.Baskets;

public sealed class Item
{
    public string ProductId { get; }

    public string Name { get; }

    public int UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public IBasketItemDiscount? ItemDiscount { get; private set; }

    public bool HasItemDiscount => ItemDiscount is not null;

    public Item(string productId, string name, int unitPrice, int quantity, IBasketItemDiscount? itemDiscount)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("Product ID is required", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required", nameof(name));
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        ProductId = productId;
        Name = name;
        UnitPrice = unitPrice;
        Quantity = quantity;
        ItemDiscount = itemDiscount;
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        Quantity += amount;
    }

    public int Total()
    {
        if (ItemDiscount is not null)
        {
            return ItemDiscount.CalculateTotal(UnitPrice, Quantity);
        }

        return UnitPrice * Quantity;
    }

    public bool Matches(Item other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(ProductId, other.ProductId, StringComparison.OrdinalIgnoreCase)
               && Equals(ItemDiscount, other.ItemDiscount);
    }

    public void ApplyDiscount(IBasketItemDiscount discount)
    {
        ItemDiscount = discount ?? throw new ArgumentNullException(nameof(discount));
    }
}



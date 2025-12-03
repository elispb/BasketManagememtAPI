using System;
using BasketManagementAPI.Domain.Discounts;

namespace BasketManagementAPI.Domain.Baskets;

public sealed class Item
{
    private const int UnassignedProductId = 0;

    public int ProductId { get; private set; }

    public string Name { get; }

    public int UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public IBasketItemDiscount? ItemDiscount { get; private set; }

    public bool HasItemDiscount => ItemDiscount is not null;

    public bool HasProductId => ProductId > UnassignedProductId;

    private Item(int productId, string name, int unitPrice, int quantity, IBasketItemDiscount? itemDiscount)
    {
        if (productId < UnassignedProductId)
        {
            throw new ArgumentOutOfRangeException(nameof(productId), "Product ID cannot be negative.");
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

    public static Item Create(string name, int unitPrice, int quantity, IBasketItemDiscount? itemDiscount)
        => new(UnassignedProductId, name, unitPrice, quantity, itemDiscount);

    public static Item FromStore(int productId, string name, int unitPrice, int quantity, IBasketItemDiscount? itemDiscount)
    {
        if (productId <= UnassignedProductId)
        {
            throw new ArgumentOutOfRangeException(nameof(productId), "Product ID must be greater than zero.");
        }

        return new Item(productId, name, unitPrice, quantity, itemDiscount);
    }

    public void AssignProductId(int productId)
    {
        if (productId <= UnassignedProductId)
        {
            throw new ArgumentOutOfRangeException(nameof(productId), "Product ID must be greater than zero.");
        }

        if (HasProductId)
        {
            throw new InvalidOperationException("Product ID is already assigned.");
        }

        ProductId = productId;
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

        if (HasProductId && other.HasProductId)
        {
            return ProductId == other.ProductId && Equals(ItemDiscount, other.ItemDiscount);
        }

        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
               && UnitPrice == other.UnitPrice
               && Equals(ItemDiscount, other.ItemDiscount);
    }

    public void ApplyDiscount(IBasketItemDiscount discount)
    {
        ItemDiscount = discount ?? throw new ArgumentNullException(nameof(discount));
    }
}



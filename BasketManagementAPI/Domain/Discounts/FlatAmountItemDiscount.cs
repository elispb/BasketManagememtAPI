using System;

namespace BasketManagementAPI.Domain.Discounts;

public sealed class FlatAmountItemDiscount : IBasketItemDiscount, IEquatable<FlatAmountItemDiscount>
{
    public string Description => $"Flat amount off {AmountTaken}p";

    public int AmountTaken { get; }

    public FlatAmountItemDiscount(int amountTaken)
    {
        if (amountTaken <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountTaken), "Discount amount must be positive.");
        }

        AmountTaken = amountTaken;
    }

    public int CalculateTotal(int unitPrice, int quantity)
    {
        var discountedUnitPrice = Math.Max(unitPrice - AmountTaken, 0);
        return discountedUnitPrice * quantity;
    }

    public bool Equals(FlatAmountItemDiscount? other)
    {
        if (other is null)
        {
            return false;
        }

        return AmountTaken == other.AmountTaken;
    }

    public override bool Equals(object? obj) => Equals(obj as FlatAmountItemDiscount);

    public override int GetHashCode() => AmountTaken.GetHashCode();
}



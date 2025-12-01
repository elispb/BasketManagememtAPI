using System;

namespace BasketManagememtAPI.Domain.Discounts;

public sealed class FlatAmountItemDiscount : IBasketItemDiscount
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
}


using System;

namespace BasketManagementAPI.Domain.Discounts;

public sealed class FlatAmountItemDiscount : IBasketItemDiscount
{
    public string Description => $"Flat amount off {AmountTaken:C}";

    public decimal AmountTaken { get; }

    public FlatAmountItemDiscount(decimal amountTaken)
    {
        if (amountTaken <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountTaken), "Discount amount must be positive.");
        }

        AmountTaken = amountTaken;
    }

    public decimal CalculateTotal(decimal unitPrice, int quantity)
    {
        var discountedUnitPrice = Math.Max(unitPrice - AmountTaken, 0);
        return discountedUnitPrice * quantity;
    }
}


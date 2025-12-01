using System;

namespace BasketManagementAPI.Domain.Discounts;

public sealed class PercentageBasketDiscount : IBasketDiscount
{
    public string Code { get; }

    public decimal Percentage { get; }

    public PercentageBasketDiscount(string code, decimal percentage)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));

        if (percentage <= 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100.");
        }

        Percentage = percentage;
    }

    public int CalculateDiscount(int eligibleAmount)
    {
        var discount = eligibleAmount * Percentage / 100m;
        return (int)Math.Round(discount, 0, MidpointRounding.AwayFromZero);
    }
}



using System.Globalization;

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

    public decimal CalculateDiscount(decimal eligibleAmount)
    {
        var discount = eligibleAmount * Percentage / 100m;
        return Math.Round(discount, 2, MidpointRounding.AwayFromZero);
    }
}


using BasketManagementAPI.Domain.Discounts;
using FluentAssertions;
using Xunit;

namespace BasketManagementAPI.Tests.ItemDiscounts;

public class PercentageBasketDiscountTests
{
    [Fact]
    public void PercentageBasketDiscount_CalculatesRoundedDiscount()
    {
        var discount = new PercentageBasketDiscount("FESTIVE", 15);

        var totalDiscount = discount.CalculateDiscount(999);

        totalDiscount.Should().Be(150);
    }

    [Fact]
    public void PercentageBasketDiscount_CalculatesZeroWhenAmountTooSmall()
    {
        var discount = new PercentageBasketDiscount("FESTIVE", 1);

        var totalDiscount = discount.CalculateDiscount(1);

        totalDiscount.Should().Be(0);
    }
}


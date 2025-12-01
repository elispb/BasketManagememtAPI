using BasketManagementAPI.Domain.Discounts;
using FluentAssertions;
using Xunit;

namespace BasketManagementAPI.Tests;

public class BasketItemDiscountTests
{
    [Fact]
    public void FlatAmountItemDiscount_CannotProduceNegativeTotals()
    {
        var discount = new FlatAmountItemDiscount(500);

        var total = discount.CalculateTotal(300, 4);

        total.Should().Be(0);
    }

    [Fact]
    public void BuyOneGetOneFreeDiscount_MakesSecondOfTwoZero()
    {
        var discount = new BuyOneGetOneFreeItemDiscount();

        var total = discount.CalculateTotal(1250, 2);

        total.Should().Be(1250);
    }

    [Fact]
    public void BuyOneGetOneFreeDiscount_DoesNotDiscountSingleItem()
    {
        var discount = new BuyOneGetOneFreeItemDiscount();

        var total = discount.CalculateTotal(1250, 1);

        total.Should().Be(1250);
    }
}

using BasketManagementAPI.Domain.Discounts;
using FluentAssertions;
using Xunit;

namespace BasketManagementAPI.Tests;

public class BasketItemDiscountTests
{
    [Fact]
    public void FlatAmountItemDiscount_CannotProduceNegativeTotals()
    {
        var discount = new FlatAmountItemDiscount(5m);

        var total = discount.CalculateTotal(3m, 4);

        total.Should().Be(0m);
    }

    [Fact]
    public void BuyOneGetOneFreeDiscount_MakesSecondOfTwoZero()
    {
        var discount = new BuyOneGetOneFreeItemDiscount();

        var total = discount.CalculateTotal(12.5m, 2);

        total.Should().Be(12.5m);
    }

    [Fact]
    public void BuyOneGetOneFreeDiscount_DoesNotDiscountSingleItem()
    {
        var discount = new BuyOneGetOneFreeItemDiscount();

        var total = discount.CalculateTotal(12.5m, 1);

        total.Should().Be(12.5m);
    }
}

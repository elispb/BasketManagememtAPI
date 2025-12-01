using BasketManagememtAPI.Domain.Discounts;
using FluentAssertions;
using Xunit;

namespace BasketManagementAPI.Tests;

public class BasketItemDiscountTests
{
    [Theory]
    [InlineData(3, 2500)]
    [InlineData(7, 5000)]
    public void BuyOneGetOneFreeDiscount_DiscountsOddQuantities(int itemCount, int expectedTotal)
    {
        var discount = new BuyOneGetOneFreeItemDiscount();

        var total = discount.CalculateTotal(1250, itemCount);

        total.Should().Be(expectedTotal);
    }

    [Theory]
    [InlineData(4, 2500)]
    public void BuyOneGetOneFreeDiscount_DiscountsEvenQuantities(int itemCount, int expectedTotal)
    {
        var discount = new BuyOneGetOneFreeItemDiscount();

        var total = discount.CalculateTotal(1250, itemCount);

        total.Should().Be(expectedTotal);
    }

    [Theory]
    [InlineData(1, 1250)]
    public void BuyOneGetOneFreeDiscount_DoesNotDiscountSingleItem(int itemCount, int expectedTotal)
    {
        var discount = new BuyOneGetOneFreeItemDiscount();

        var total = discount.CalculateTotal(1250, itemCount);

        total.Should().Be(expectedTotal);
    }
}

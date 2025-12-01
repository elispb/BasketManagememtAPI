using BasketManagementAPI.Domain.Discounts;
using FluentAssertions;
using Xunit;

namespace BasketManagementAPI.Tests.ItemDiscounts
{
    public class FlatAmount
    {
        [Fact]
        public void FlatAmountItemDiscount_CannotProduceNegativeTotals()
        {
            var discount = new FlatAmountItemDiscount(500);

            var total = discount.CalculateTotal(300, 4);

            total.Should().Be(0);
        }

        [Fact]
        public void FlatAmountItemDiscount_DiscountsSingleItem()
        {
            var discount = new FlatAmountItemDiscount(5);

            var total = discount.CalculateTotal(10, 1);

            total.Should().Be(5);
        }

        [Fact]
        public void FlatAmountItemDiscount_DiscountsMultipleItems()
        {
            var discount = new FlatAmountItemDiscount(1);

            var total = discount.CalculateTotal(10, 3);

            total.Should().Be(27);
        }
    }
}


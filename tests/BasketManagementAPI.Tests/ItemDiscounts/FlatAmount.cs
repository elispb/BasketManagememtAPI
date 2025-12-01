using BasketManagementAPI.Domain.Discounts;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}

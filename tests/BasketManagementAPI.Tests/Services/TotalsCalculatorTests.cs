using System;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Services;
using FluentAssertions;
using Moq;

namespace BasketManagementAPI.Tests.Services;

public sealed class TotalsCalculatorTests
{
    [Fact]
    public async Task CalculateAsync_AppliesDiscountFromCatalog()
    {
        var basketId = Guid.NewGuid();
        var basket = new Basket(basketId);
        basket.AddOrUpdateItem(new Item("p-1", "Item 1", 100, 2, null));

        var definitionId = Guid.NewGuid();
        basket.ApplyDiscount(new PercentageBasketDiscount("SAVE50", 50m), definitionId);

        var definition = new DiscountDefinition(definitionId, "SAVE50", 50m, null, true);
        var catalog = new Mock<IDiscountCatalog>();
        catalog.Setup(c => c.GetActiveDefinitionAsync(definitionId)).ReturnsAsync(definition);

        var calculator = new TotalsCalculator(catalog.Object);

        var totals = await calculator.CalculateAsync(basket);

        totals.Discount.Should().Be(100);
        catalog.Verify(c => c.GetActiveDefinitionAsync(definitionId), Times.Once);
    }
}


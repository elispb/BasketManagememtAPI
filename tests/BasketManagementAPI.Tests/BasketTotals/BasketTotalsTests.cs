using System;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;
using BasketManagementAPI.Services;
using FluentAssertions;
using Moq;

namespace BasketManagementAPI.Tests.BasketTotals;

public class BasketTotalsTests
{
    private readonly Mock<IDiscountCatalog> _discountCatalogMock;
    private readonly ITotalsCalculator _totalsCalculator;

    public BasketTotalsTests()
    {
        _discountCatalogMock = new Mock<IDiscountCatalog>();
        _discountCatalogMock
            .Setup(c => c.GetActiveDefinitionAsync(It.IsAny<Guid?>()))
            .ReturnsAsync((DiscountDefinition?)null);

        _totalsCalculator = new TotalsCalculator(_discountCatalogMock.Object);
    }

    [Fact]
    public async Task CalculateAsync_IncludesSubtotalDiscountShippingVat()
    {
        var discountDefinitionId = Guid.NewGuid();
        var basket = new Basket();
        basket.AddOrUpdateItem(Item.Create("Item without discount", 500, 1, null));
        basket.AddOrUpdateItem(Item.Create("Discounted item", 200, 1, new FlatAmountItemDiscount(50)));
        basket.SetShipping(new ShippingDetails("UK", 20));
        basket.ApplyDiscount(new PercentageBasketDiscount("VACAY", 10), discountDefinitionId);
        _discountCatalogMock
            .Setup(c => c.GetActiveDefinitionAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "VACAY", 10, null, true));

        var totals = await _totalsCalculator.CalculateAsync(basket);

        totals.Subtotal.Should().Be(650);
        totals.Discount.Should().Be(50);
        totals.Shipping.Should().Be(20);
        totals.TotalWithoutVat.Should().Be(620);
        totals.VatAmount.Should().Be(120);
        totals.TotalWithVat.Should().Be(740);
    }

    [Fact]
    public async Task CalculateAsync_TotalWithoutVat_FloorsAtZero()
    {
        var discountDefinitionId = Guid.NewGuid();
        var basket = new Basket();
        basket.AddOrUpdateItem(Item.Create("Expensive item", 100, 1, null));
        basket.ApplyDiscount(new PercentageBasketDiscount("FREE", 100), discountDefinitionId);
        basket.SetShipping(new ShippingDetails("UK", 20));
        _discountCatalogMock
            .Setup(c => c.GetActiveDefinitionAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "FREE", 100, null, true));

        var totals = await _totalsCalculator.CalculateAsync(basket);

        totals.TotalWithoutVat.Should().Be(20);
        totals.VatAmount.Should().Be(0);
        totals.TotalWithVat.Should().Be(20);
    }

    [Fact]
    public async Task CalculateAsync_HandlesVeryLargeAmounts()
    {
        var discountDefinitionId = Guid.NewGuid();
        var basket = new Basket();
        basket.AddOrUpdateItem(Item.Create("Large item #1", 900_000_000, 2, null));
        basket.ApplyDiscount(new PercentageBasketDiscount("BULK", 10), discountDefinitionId);
        basket.SetShipping(new ShippingDetails("UK", 0));
        _discountCatalogMock
            .Setup(c => c.GetActiveDefinitionAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "BULK", 10, null, true));

        var totals = await _totalsCalculator.CalculateAsync(basket);

        totals.Subtotal.Should().Be(1_800_000_000);
        totals.Discount.Should().Be(180_000_000);
        totals.TotalWithoutVat.Should().Be(1_620_000_000);
        totals.VatAmount.Should().Be(324_000_000);
        totals.TotalWithVat.Should().Be(1_944_000_000);
    }
}
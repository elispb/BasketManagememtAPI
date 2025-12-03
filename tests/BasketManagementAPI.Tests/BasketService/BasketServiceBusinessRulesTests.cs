using System;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;
using BasketManagementAPI.Repositories;
using BasketManagementAPI.Services;
using BasketManagementAPI.Shipping;
using FluentAssertions;
using Moq;
using Xunit;

namespace BasketManagementAPI.Tests.BasketService;

public class BasketServiceBusinessRulesTests
{
    [Fact]
    public async Task GetTotalsAsync_AppliesBasketDiscountOnlyToItemsWithoutItemDiscount()
    {
        // Arrange
        var basket = new Basket();
        basket.AddOrUpdateItem(new Item("FULL", "Full price item", 150, 1, null));
        basket.AddOrUpdateItem(new Item("DISCOUNTED", "Manually discounted item", 100, 1, new FlatAmountItemDiscount(30)));
        var discountDefinitionId = Guid.NewGuid();
        basket.ApplyDiscount(new PercentageBasketDiscount("HALF", 50), discountDefinitionId);

        var repositoryMock = new Mock<IBasketRepository>();
        repositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(basket);

        var discountDefinitionRepositoryMock = new Mock<IDiscountDefinitionRepository>();
        discountDefinitionRepositoryMock
            .Setup(r => r.GetByIdAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "HALF", 50, null, true));

        var shippingPolicyMock = new Mock<IShippingPolicy>();

        var service = new BasketService(
            repositoryMock.Object,
            shippingPolicyMock.Object,
            discountDefinitionRepositoryMock.Object);

        // Act
        var totals = await service.GetTotalsAsync(Guid.NewGuid());

        // Assert: eligible amount only includes the item without an item discount (150 -> 50% = 75)
        totals.Discount.Should().Be(75);
        totals.Subtotal.Should().Be(220);
    }

    [Fact]
    public async Task GetTotalsAsync_IgnoresInactiveDiscountDefinitions()
    {
        // Arrange
        var basket = new Basket();
        basket.AddOrUpdateItem(new Item("REG", "Regular item", 200, 1, null));
        var discountDefinitionId = Guid.NewGuid();
        basket.ApplyDiscount(new PercentageBasketDiscount("INACTIVE", 25), discountDefinitionId);

        var repositoryMock = new Mock<IBasketRepository>();
        repositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(basket);

        var discountDefinitionRepositoryMock = new Mock<IDiscountDefinitionRepository>();
        discountDefinitionRepositoryMock
            .Setup(r => r.GetByIdAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "INACTIVE", 25, null, false));

        var shippingPolicyMock = new Mock<IShippingPolicy>();

        var service = new BasketService(
            repositoryMock.Object,
            shippingPolicyMock.Object,
            discountDefinitionRepositoryMock.Object);

        // Act
        var totals = await service.GetTotalsAsync(Guid.NewGuid());

        // Assert: inactive definitions should not apply any discount
        totals.Discount.Should().Be(0);
        totals.TotalWithoutVat.Should().Be(200);
    }

    [Fact]
    public async Task GetTotalsAsync_DoesNotApplyDiscount_WhenEligibleAmountIsZero()
    {
        // Arrange
        var basket = new Basket();
        basket.AddOrUpdateItem(new Item("DISCOUNTED-A", "Manually discounted item", 100, 1, new FlatAmountItemDiscount(50)));
        basket.AddOrUpdateItem(new Item("DISCOUNTED-B", "Manually discounted item", 80, 1, new FlatAmountItemDiscount(30)));
        var discountDefinitionId = Guid.NewGuid();
        basket.ApplyDiscount(new PercentageBasketDiscount("NONE", 100), discountDefinitionId);

        var repositoryMock = new Mock<IBasketRepository>();
        repositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(basket);

        var discountDefinitionRepositoryMock = new Mock<IDiscountDefinitionRepository>();
        discountDefinitionRepositoryMock
            .Setup(r => r.GetByIdAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "NONE", 100, null, true));

        var shippingPolicyMock = new Mock<IShippingPolicy>();

        var service = new BasketService(
            repositoryMock.Object,
            shippingPolicyMock.Object,
            discountDefinitionRepositoryMock.Object);

        // Act
        var totals = await service.GetTotalsAsync(Guid.NewGuid());

        // Assert: when every item already has an item discount, basket discount should be zero
        totals.Discount.Should().Be(0);
    }
}


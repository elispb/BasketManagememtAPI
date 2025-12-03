using System;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;
using BasketManagementAPI.Repositories;
using BasketManagementAPI.Services;
using FluentAssertions;
using Moq;

namespace BasketManagementAPI.Tests.BasketServiceTests;

public sealed class BasketServiceDiscountCatalogTests
{
    [Fact]
    public async Task ApplyDiscountCodeAsync_UsesDiscountCatalogResult()
    {
        var basketId = Guid.NewGuid();
        var basket = new Basket(basketId);
        var repository = new Mock<IBasketRepository>();
        repository.Setup(r => r.GetAsync(basketId)).ReturnsAsync(basket);

        var definition = new DiscountDefinition(Guid.NewGuid(), "SAVE20", 20m, null, true);
        var catalog = new Mock<IDiscountCatalog>();
        catalog.Setup(c => c.EnsureDefinitionAsync("SAVE20", 20m)).ReturnsAsync(definition);

        var basketService = new BasketService(
            repository.Object,
            Mock.Of<IShippingPolicy>(),
            Mock.Of<ITotalsCalculator>(),
            catalog.Object);

        var result = await basketService.ApplyDiscountCodeAsync(basketId, "SAVE20", 20m);

        result.DiscountDefinitionId.Should().Be(definition.Id);
        result.BasketDiscount.Should().NotBeNull();
        catalog.Verify(c => c.EnsureDefinitionAsync("SAVE20", 20m), Times.Once);
        repository.Verify(r => r.SaveAsync(basket), Times.Once);
    }
}


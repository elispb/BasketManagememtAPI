using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;
using BasketManagementAPI.Repositories;
using BasketManagementAPI.Services;
using BasketManagementAPI.Shipping;
using FluentAssertions;
using Moq;

namespace BasketManagementAPI.Tests.BasketTotals;

public class BasketTotalsTests
{
    private readonly Mock<IBasketRepository> _repositoryMock;
    private readonly Mock<IShippingPolicy> _shippingPolicyMock;
    private readonly Mock<IDiscountDefinitionRepository> _discountDefinitionRepositoryMock;

    public BasketTotalsTests()
    {
        _repositoryMock = new Mock<IBasketRepository>();
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Basket>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveAsync(It.IsAny<Basket>())).Returns(Task.CompletedTask);

        _shippingPolicyMock = new Mock<IShippingPolicy>();
        _discountDefinitionRepositoryMock = new Mock<IDiscountDefinitionRepository>();
        _discountDefinitionRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((DiscountDefinition?)null);
        _discountDefinitionRepositoryMock
            .Setup(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(Guid.NewGuid());
        _shippingPolicyMock.Setup(p => p.ResolveAsync(It.IsAny<string>()))
            .ReturnsAsync(new ShippingDetails("UK", 0));
    }

    [Fact]
    public async Task GetTotalsAsync_IncludesSubtotalDiscountShippingVat()
    {
        var discountDefinitionId = Guid.NewGuid();
        var basket = new Basket();
        basket.AddOrUpdateItem(new Item("A1", "Item without discount", 500, 1, null));
        basket.AddOrUpdateItem(new Item("D1", "Discounted item", 200, 1, new FlatAmountItemDiscount(50)));
        basket.SetShipping(new ShippingDetails("UK", 20));
        basket.ApplyDiscount(new PercentageBasketDiscount("VACAY", 10), discountDefinitionId);
        _discountDefinitionRepositoryMock
            .Setup(r => r.GetByIdAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "VACAY", 10, null, true));

        var service = CreateServiceWithBasket(basket);

        var totals = await service.GetTotalsAsync(basket.Id);

        // Total goods including item level discount
        totals.Subtotal.Should().Be(650);
        // Discount from basket level discount only
        totals.Discount.Should().Be(50);
        totals.Shipping.Should().Be(20);
        // Total without VAT after all discounts and shipping
        totals.TotalWithoutVat.Should().Be(620);
        // VAT applies to goods after basket discounts but excludes shipping
        totals.VatAmount.Should().Be(120);
        totals.TotalWithVat.Should().Be(740);
    }

    [Fact]
    public async Task GetTotalsAsync_TotalWithoutVat_FloorsAtZero()
    {
        var discountDefinitionId = Guid.NewGuid();
        var basket = new Basket();
        basket.AddOrUpdateItem(new Item("E1", "Expensive item", 100, 1, null));
        basket.ApplyDiscount(new PercentageBasketDiscount("FREE", 100), discountDefinitionId);
        basket.SetShipping(new ShippingDetails("UK", 20));
        _discountDefinitionRepositoryMock
            .Setup(r => r.GetByIdAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "FREE", 100, null, true));

        var service = CreateServiceWithBasket(basket);

        var totals = await service.GetTotalsAsync(basket.Id);

        totals.TotalWithoutVat.Should().Be(20);
        totals.VatAmount.Should().Be(0);
        totals.TotalWithVat.Should().Be(20);
    }

    [Fact]
    public async Task GetTotalsAsync_HandlesVeryLargeAmounts()
    {
        var discountDefinitionId = Guid.NewGuid();
        var basket = new Basket();
        basket.AddOrUpdateItem(new Item("L1", "Large item #1", 900_000_000, 2, null));
        basket.ApplyDiscount(new PercentageBasketDiscount("BULK", 10), discountDefinitionId);
        basket.SetShipping(new ShippingDetails("UK", 0));
        _discountDefinitionRepositoryMock
            .Setup(r => r.GetByIdAsync(discountDefinitionId))
            .ReturnsAsync(new DiscountDefinition(discountDefinitionId, "BULK", 10, null, true));

        var service = CreateServiceWithBasket(basket);

        var totals = await service.GetTotalsAsync(basket.Id);

        totals.Subtotal.Should().Be(1_800_000_000);
        totals.Discount.Should().Be(180_000_000);
        totals.TotalWithoutVat.Should().Be(1_620_000_000);
        totals.VatAmount.Should().Be(324_000_000);
        totals.TotalWithVat.Should().Be(1_944_000_000);
    }

    private BasketService CreateServiceWithBasket(Basket basket)
    {
        _repositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(basket);
        return new BasketService(_repositoryMock.Object, _shippingPolicyMock.Object, _discountDefinitionRepositoryMock.Object);
    }

}
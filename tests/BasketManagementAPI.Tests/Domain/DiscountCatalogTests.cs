using System;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Repositories;
using FluentAssertions;
using Moq;

namespace BasketManagementAPI.Tests.Domain.Discounts;

public sealed class DiscountCatalogTests
{
    [Fact]
    public async Task EnsureDefinitionAsync_ReturnsActiveDefinition()
    {
        var id = Guid.NewGuid();
        var definition = new DiscountDefinition(id, "SAVE", 20m, null, true);
        var repository = new Mock<IDiscountDefinitionRepository>();
        repository.Setup(r => r.GetByCodeAsync("SAVE")).ReturnsAsync((DiscountDefinition?)null);
        repository.Setup(r => r.UpsertAsync("SAVE", 20m)).ReturnsAsync(id);
        repository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(definition);

        var catalog = new DiscountCatalog(repository.Object);

        var result = await catalog.EnsureDefinitionAsync("SAVE", 20m);

        result.Should().Be(definition);
    }

    [Fact]
    public async Task EnsureDefinitionAsync_ThrowsWhenDefinitionIsInactive()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<IDiscountDefinitionRepository>();
        repository.Setup(r => r.GetByCodeAsync("SAVE")).ReturnsAsync((DiscountDefinition?)null);
        repository.Setup(r => r.UpsertAsync("SAVE", 20m)).ReturnsAsync(id);
        repository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new DiscountDefinition(id, "SAVE", 20m, null, false));

        var catalog = new DiscountCatalog(repository.Object);

        await FluentActions.Awaiting(() => catalog.EnsureDefinitionAsync("SAVE", 20m))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetActiveDefinitionAsync_ReturnsNullWhenDefinitionMissing()
    {
        var repository = new Mock<IDiscountDefinitionRepository>();
        var catalog = new DiscountCatalog(repository.Object);

        var result = await catalog.GetActiveDefinitionAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task EnsureDefinitionAsync_UsesStoredDefinitionWhenAvailable()
    {
        var id = Guid.NewGuid();
        var definition = new DiscountDefinition(id, "SAVE", 20m, null, true);
        var repository = new Mock<IDiscountDefinitionRepository>();
        repository.Setup(r => r.GetByCodeAsync("SAVE")).ReturnsAsync(definition);

        var catalog = new DiscountCatalog(repository.Object);

        var result = await catalog.EnsureDefinitionAsync("SAVE", 20m);

        result.Should().Be(definition);
        repository.Verify(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task EnsureDefinitionAsync_ThrowsWhenStoredPercentageDiffers()
    {
        var id = Guid.NewGuid();
        var definition = new DiscountDefinition(id, "SAVE", 20m, null, true);
        var repository = new Mock<IDiscountDefinitionRepository>();
        repository.Setup(r => r.GetByCodeAsync("SAVE")).ReturnsAsync(definition);

        var catalog = new DiscountCatalog(repository.Object);

        await FluentActions.Awaiting(() => catalog.EnsureDefinitionAsync("SAVE", 50m))
            .Should()
            .ThrowAsync<InvalidOperationException>();

        repository.Verify(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
    }
}


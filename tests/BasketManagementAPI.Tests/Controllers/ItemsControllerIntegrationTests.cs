using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BasketManagementAPI.Contracts.Requests;
using BasketManagementAPI.Contracts.Responses;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BasketManagementAPI.Tests.Controllers;

[Collection("DatabaseContainer")]
public sealed class ItemsControllerIntegrationTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TestWebApplicationFactory _factory;
    private readonly SqlServerDockerFixture _databaseFixture;

    public ItemsControllerIntegrationTests(SqlServerDockerFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        _factory = new TestWebApplicationFactory(_databaseFixture.ConnectionString);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddItems_AllowsGettingItem()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);
        const string productId = "SKU-ITEM-001";

        await AddItemsAsync(client, basketId, productId, 150, 1);

        var getResponse = await client.GetAsync($"/api/baskets/{basketId}/items/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await getResponse.Content.ReadFromJsonAsync<ItemResponse>(JsonOptions);
        item.Should().NotBeNull();
        item!.ProductId.Should().Be(productId);
        item.Quantity.Should().Be(1);
        item.Total.Should().Be(150);
    }

    [Fact]
    public async Task ApplyingItemDiscount_AdjustsLineTotalAndPrice()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);
        const string productId = "SKU-ITEM-002";

        await AddItemsAsync(client, basketId, productId, 120, 2);

        var discountResponse = await client.PatchAsJsonAsync(
            $"/api/baskets/{basketId}/items/{productId}/discount",
            new ItemDiscountRequest(ItemDiscountType.FlatAmount, 25));

        discountResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var discounted = await discountResponse.Content.ReadFromJsonAsync<ItemResponse>(JsonOptions);
        discounted.Should().NotBeNull();
        discounted!.Total.Should().Be(190);

        var priceResponse = await client.GetAsync($"/api/baskets/{basketId}/items/{productId}/price");
        priceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var price = await priceResponse.Content.ReadFromJsonAsync<ItemPriceResponse>(JsonOptions);
        price.Should().NotBeNull();
        price!.LineTotal.Should().Be(discounted.Total);
    }

    [Fact]
    public async Task RemovingItem_ReturnsNotFoundAfterDelete()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);
        const string productId = "SKU-ITEM-003";

        await AddItemsAsync(client, basketId, productId, 75, 1);

        var deleteResponse = await client.DeleteAsync($"/api/baskets/{basketId}/items/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/baskets/{basketId}/items/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> CreateBasketAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/baskets", null);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return payload.GetProperty("basketId").GetGuid();
    }

    private static async Task AddItemsAsync(HttpClient client, Guid basketId, string productId, int unitPrice, int quantity)
    {
        var addItemsRequest = new AddItemsRequest(new[]
        {
            new AddItemRequest(productId, "Integration item", unitPrice, quantity, null)
        });

        var response = await client.PostAsJsonAsync($"/api/baskets/{basketId}/items", addItemsRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}


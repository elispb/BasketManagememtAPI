using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BasketManagementAPI.Contracts.Requests;
using BasketManagementAPI.Contracts.Responses;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BasketManagementAPI.Tests.Controllers;

[Collection("DatabaseContainer")]
public sealed class BasketsControllerIntegrationTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TestWebApplicationFactory _factory;
    private readonly SqlServerDockerFixture _databaseFixture;

    public BasketsControllerIntegrationTests(SqlServerDockerFixture databaseFixture)
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
    public async Task CreateBasket_ReturnsCreatedWithBasketId()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/baskets", null);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("basketId").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AddItemsAndGetBasket_ReturnsPersistedItems()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);

        await AddSampleItemAsync(client, basketId);

        var getResponse = await client.GetAsync($"/api/baskets/{basketId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var basket = await getResponse.Content.ReadFromJsonAsync<BasketResponse>(JsonOptions);
        basket.Should().NotBeNull();
        basket!.Items.Should().ContainSingle(item =>
            item.ProductId > 0
            && item.Name == "Integration sample item"
            && item.Quantity == 2
            && item.UnitPrice == 200);
    }

    [Fact]
    public async Task AddShipping_ReturnsTotalsWithShippingCost()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);
        await AddSampleItemAsync(client, basketId);

        var shippingResponse = await client.PatchAsJsonAsync(
            $"/api/baskets/{basketId}/shipping",
            new AddShippingRequest("uk"));

        shippingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var totals = await shippingResponse.Content.ReadFromJsonAsync<PriceResponse>(JsonOptions);
        totals.Should().NotBeNull();
        totals!.Shipping.Should().Be(499);
    }

    [Fact]
    public async Task AddShipping_ReturnsBadRequest_WhenCountryMissing()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);

        var response = await client.PatchAsJsonAsync(
            $"/api/baskets/{basketId}/shipping",
            new AddShippingRequest(string.Empty));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        validation.Should().NotBeNull();
        validation!.Errors["CountryCode"].Should().Contain("Country code is required for shipping.");
    }

    [Fact]
    public async Task AddShipping_AcceptsNumericCountryCode()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);
        await AddSampleItemAsync(client, basketId);

        var shippingResponse = await client.PatchAsJsonAsync(
            $"/api/baskets/{basketId}/shipping",
            new AddShippingRequest("1"));

        shippingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var totals = await shippingResponse.Content.ReadFromJsonAsync<PriceResponse>(JsonOptions);
        totals.Should().NotBeNull();
        totals!.Shipping.Should().Be(499);
    }

    [Fact]
    public async Task ApplyDiscount_ReturnsTotalsWithDiscount()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);
        await AddSampleItemAsync(client, basketId);

        var discountResponse = await client.PatchAsJsonAsync(
            $"/api/baskets/{basketId}/discount",
            new ApplyDiscountRequest("TEST10", 10));

        discountResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/baskets/{basketId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var basket = await getResponse.Content.ReadFromJsonAsync<BasketResponse>(JsonOptions);
        basket.Should().NotBeNull();
        basket!.DiscountCode.Should().Be("TEST10");
        basket.Totals.Discount.Should().Be(40);
    }

    [Fact]
    public async Task ApplyDiscount_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var client = _factory.CreateClient();
        var basketId = await CreateBasketAsync(client);

        var response = await client.PatchAsJsonAsync(
            $"/api/baskets/{basketId}/discount",
            new ApplyDiscountRequest(string.Empty, 0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        validation.Should().NotBeNull();
        validation!.Errors["Code"].Should().Contain("Discount code is required.");
        validation.Errors["Percentage"].Should().Contain("Percentage must be greater than zero.");
    }

    [Fact]
    public async Task GetBaskets_ReturnsAllExistingBaskets()
    {
        var client = _factory.CreateClient();
        var firstBasketId = await CreateBasketAsync(client);
        await AddSampleItemAsync(client, firstBasketId);
        var secondBasketId = await CreateBasketAsync(client);

        var response = await client.GetAsync("/api/baskets");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var baskets = await response.Content.ReadFromJsonAsync<BasketResponse[]>(JsonOptions);
        baskets.Should().NotBeNull();

        baskets!.Select(basket => basket.Id).Should().Contain(new[] { firstBasketId, secondBasketId });
        var persisted = baskets.Single(basket => basket.Id == firstBasketId);
        persisted.Items.Should().ContainSingle(item => item.Name == "Integration sample item");
    }

    private static async Task<Guid> CreateBasketAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/baskets", null);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return payload.GetProperty("basketId").GetGuid();
    }

    private static async Task AddSampleItemAsync(HttpClient client, Guid basketId)
    {
        var addItemsRequest = new AddItemsRequest(new[]
        {
            new AddItemRequest("Integration sample item", 200, 2, null)
        });

        var response = await client.PostAsJsonAsync($"/api/baskets/{basketId}/items", addItemsRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdItems = await response.Content.ReadFromJsonAsync<ItemResponse[]>(JsonOptions);
        createdItems.Should().NotBeNull();
        createdItems!.Single().ProductId.Should().BeGreaterThan(0);
    }
}


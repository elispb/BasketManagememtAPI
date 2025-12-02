using System;
using System.Collections.Generic;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Repositories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BasketManagementAPI.Tests.Repositories;

[Collection("DatabaseContainer")]
public sealed class SqlBasketRepositoryTests
{
    private const string DefaultConnectionString =
        "Server=localhost,1433;Database=BasketDb;User Id=sa;Password=Str0ng!Passw0rd;TrustServerCertificate=True;";

    private readonly SqlBasketRepository _repository;
    private readonly string _connectionString;

    public SqlBasketRepositoryTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("BasketDb__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("BasketDbConnectionString")
            ?? DefaultConnectionString;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString
                })
            .Build();

        _repository = new SqlBasketRepository(configuration);
    }

    [Fact]
    public async Task CreateAndGetAsync_RoundTripsBasket()
    {
        var basket = new Basket();
        basket.AddOrUpdateItem(new Item("SKU-DB-TEST", "Integration test item", 100, 1, null));

        try
        {
            await _repository.CreateAsync(basket);

            var persisted = await _repository.GetAsync(basket.Id);

            persisted.Id.Should().Be(basket.Id);
            persisted.Items.Should().ContainSingle(item =>
                item.ProductId == "SKU-DB-TEST"
                && item.Name == "Integration test item"
                && item.Quantity == 1
                && item.UnitPrice == 100);
        }
        finally
        {
            await DeleteBasketAsync(basket.Id);
        }
    }

    private async Task DeleteBasketAsync(Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [dbo].[Baskets] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync();
    }
}


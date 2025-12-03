using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;
using Microsoft.Data.SqlClient;

namespace BasketManagementAPI.Repositories;

public sealed class SqlBasketRepository : IBasketRepository
{
    private readonly string _connectionString;

    public SqlBasketRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A connection string named 'DefaultConnection' was not found.");
    }

    public async Task<Basket> GetAsync(Guid id)
    {
        await using var connection = await CreateOpenConnectionAsync();

        var basket = await LoadBasketAsync(connection, id);
        await LoadItemsAsync(connection, basket);
        await LoadShippingAsync(connection, basket);

        return basket;
    }

    public async Task<IReadOnlyCollection<Basket>> GetAllAsync()
    {
        await using var connection = await CreateOpenConnectionAsync();

        var basketIds = new List<Guid>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT [Id] FROM [dbo].[Baskets];";
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                basketIds.Add(reader.GetGuid(0));
            }
        }

        var baskets = new List<Basket>(basketIds.Count);
        foreach (var basketId in basketIds)
        {
            var basket = await LoadBasketAsync(connection, basketId);
            await LoadItemsAsync(connection, basket);
            await LoadShippingAsync(connection, basket);
            baskets.Add(basket);
        }

        return baskets;
    }

    public async Task CreateAsync(Basket basket)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "usp_CreateBasket";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", basket.Id);
            AddBasketParameters(command, basket);

            await command.ExecuteNonQueryAsync();

            foreach (var item in basket.Items)
            {
                var resolvedId = await UpsertItemAsync(connection, basket.Id, item, transaction);
                if (!item.HasProductId)
                {
                    item.AssignProductId(resolvedId);
                }
            }

            if (basket.ShippingDetails is not null)
            {
                await UpsertBasketShippingAsync(connection, basket.Id, basket.ShippingDetails, transaction);
            }

            await transaction.CommitAsync();
        }
        catch (SqlException ex) when (ex.Number == 2627)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Basket '{basket.Id}' already exists.", ex);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task SaveAsync(Basket basket)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await UpdateBasketAsync(connection, basket, transaction);

            var existingProductIds = await LoadExistingProductIdsAsync(connection, basket.Id, transaction);
            var currentProductIds = new HashSet<int>(
                basket.Items
                    .Where(item => item.HasProductId)
                    .Select(item => item.ProductId));

            foreach (var productId in existingProductIds.Except(currentProductIds))
            {
                await DeleteItemInternalAsync(connection, basket.Id, productId, transaction);
            }

            foreach (var item in basket.Items)
            {
                var resolvedId = await UpsertItemAsync(connection, basket.Id, item, transaction);
                if (!item.HasProductId)
                {
                    item.AssignProductId(resolvedId);
                }
            }

            await HandleBasketShippingAsync(connection, basket, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static void AddBasketParameters(SqlCommand command, Basket basket)
    {
        command.Parameters.AddWithValue(
            "@DiscountDefinitionId",
            basket.DiscountDefinitionId.HasValue ? (object)basket.DiscountDefinitionId.Value : DBNull.Value);
    }

    private static Item BuildItem(SqlDataReader reader)
    {
        var productId = reader.GetInt32(0);
        var name = reader.GetString(1);
        var unitPrice = reader.GetInt32(2);
        var quantity = reader.GetInt32(3);
        var discountType = reader.IsDBNull(4) ? null : (byte?)reader.GetByte(4);
        var discountAmount = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5);
        var discount = ItemDiscountFactory.Create(discountType, discountAmount);

        return Item.FromStore(productId, name, unitPrice, quantity, discount);
    }

    private static async Task<Basket> LoadBasketAsync(SqlConnection connection, Guid id)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                b.[DiscountDefinitionId],
                d.[Code],
                d.[Percentage]
            FROM [dbo].[Baskets] b
            LEFT JOIN [dbo].[DiscountDefinitions] d
                ON b.[DiscountDefinitionId] = d.[Id]
            WHERE b.[Id] = @Id;
            """;
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new KeyNotFoundException($"Basket '{id}' not found.");
        }

        Guid? definitionId = reader.IsDBNull(0) ? null : reader.GetGuid(0);
        var code = reader.IsDBNull(1) ? null : reader.GetString(1);
        decimal? percentage = reader.IsDBNull(2) ? null : reader.GetDecimal(2);

        var basket = new Basket(id);

        var discount = BuildBasketDiscount(code, percentage);
        if (discount is not null)
        {
            basket.ApplyDiscount(discount, definitionId);
        }

        await reader.CloseAsync();
        return basket;
    }

    private static IBasketDiscount? BuildBasketDiscount(string? code, decimal? percentage)
    {
        if (string.IsNullOrWhiteSpace(code) || percentage is null)
        {
            return null;
        }

        return new PercentageBasketDiscount(code, percentage.Value);
    }

    private static async Task LoadItemsAsync(SqlConnection connection, Basket basket)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                [ProductId],
                [Name],
                [UnitPrice],
                [Quantity],
                [ItemDiscountType],
                [ItemDiscountAmount]
            FROM [dbo].[Items]
            WHERE [BasketId] = @BasketId;
            """;
        command.Parameters.AddWithValue("@BasketId", basket.Id);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = BuildItem(reader);
            basket.AddOrUpdateItem(item);
        }
    }

    public async Task<Item?> GetItemAsync(Guid basketId, int productId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                [ProductId],
                [Name],
                [UnitPrice],
                [Quantity],
                [ItemDiscountType],
                [ItemDiscountAmount]
            FROM [dbo].[Items]
            WHERE [BasketId] = @BasketId
              AND [ProductId] = @ProductId;
            """;
        command.Parameters.AddWithValue("@BasketId", basketId);
        command.Parameters.AddWithValue("@ProductId", productId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return BuildItem(reader);
    }

    public async Task<bool> DeleteItemAsync(Guid basketId, int productId)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "usp_DeleteItem";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BasketId", basketId);
        command.Parameters.AddWithValue("@ProductId", productId);

        var returnParam = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
        returnParam.Direction = ParameterDirection.ReturnValue;
        await command.ExecuteNonQueryAsync();
        return returnParam.Value is int rows && rows > 0;
    }

    public async Task<Item?> UpdateItemDiscountAsync(
        Guid basketId,
        int productId,
        byte? itemDiscountType,
        int? itemDiscountAmount)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "usp_UpdateItemDiscount";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BasketId", basketId);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue(
            "@ItemDiscountType",
            itemDiscountType.HasValue ? (object)itemDiscountType.Value : DBNull.Value);
        command.Parameters.AddWithValue(
            "@ItemDiscountAmount",
            itemDiscountAmount.HasValue ? (object)itemDiscountAmount.Value : DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return BuildItem(reader);
    }

    private static async Task LoadShippingAsync(SqlConnection connection, Basket basket)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                [CountryCode],
                [Cost]
            FROM [dbo].[BasketShipping]
            WHERE [BasketId] = @BasketId;
            """;
        command.Parameters.AddWithValue("@BasketId", basket.Id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var countryCodeRaw = reader.GetString(0);
            var cost = reader.GetInt32(1);
            var countryCode = CountryCodeParser.TryParse(countryCodeRaw, out var parsed)
                ? parsed
                : CountryCode.Unknown;
            basket.SetShipping(new ShippingDetails(countryCode, cost));
        }
    }

    private static async Task UpdateBasketAsync(SqlConnection connection, Basket basket, SqlTransaction transaction)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "usp_UpdateBasket";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Id", basket.Id);
        AddBasketParameters(command, basket);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task HandleBasketShippingAsync(SqlConnection connection, Basket basket, SqlTransaction transaction)
    {
        if (basket.ShippingDetails is null)
        {
            await DeleteBasketShippingAsync(connection, basket.Id, transaction);
            return;
        }

        await UpsertBasketShippingAsync(connection, basket.Id, basket.ShippingDetails, transaction);
    }

    private static async Task UpsertBasketShippingAsync(SqlConnection connection, Guid basketId, ShippingDetails shipping, SqlTransaction transaction)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "usp_UpsertBasketShipping";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BasketId", basketId);
        command.Parameters.AddWithValue("@CountryCode", (int)shipping.CountryCode);
        command.Parameters.AddWithValue("@Cost", shipping.Cost);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DeleteBasketShippingAsync(SqlConnection connection, Guid basketId, SqlTransaction transaction)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "usp_DeleteBasketShipping";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BasketId", basketId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<HashSet<int>> LoadExistingProductIdsAsync(SqlConnection connection, Guid basketId, SqlTransaction transaction)
    {
        var result = new HashSet<int>();

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT
                [ProductId]
            FROM [dbo].[Items]
            WHERE [BasketId] = @BasketId;
            """;
        command.Parameters.AddWithValue("@BasketId", basketId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetInt32(0));
        }

        return result;
    }

    private static async Task DeleteItemInternalAsync(SqlConnection connection, Guid basketId, int productId, SqlTransaction transaction)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "usp_DeleteItem";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BasketId", basketId);
        command.Parameters.AddWithValue("@ProductId", productId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> UpsertItemAsync(SqlConnection connection, Guid basketId, Item item, SqlTransaction transaction)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "usp_UpsertItem";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BasketId", basketId);
        command.Parameters.AddWithValue(
            "@ProductId",
            item.HasProductId ? (object)item.ProductId : DBNull.Value);
        command.Parameters.AddWithValue("@Name", item.Name);
        command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
        command.Parameters.AddWithValue("@Quantity", item.Quantity);

        var (type, amount) = ItemDiscountFactory.ToPersistedData(item.ItemDiscount);
        command.Parameters.AddWithValue(
            "@ItemDiscountType",
            type.HasValue ? (object)type.Value : DBNull.Value);
        command.Parameters.AddWithValue(
            "@ItemDiscountAmount",
            amount.HasValue ? (object)amount.Value : DBNull.Value);
        var resolvedProductId = command.Parameters.Add("@ResolvedProductId", SqlDbType.Int);
        resolvedProductId.Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync();

        return resolvedProductId.Value is int id ? id : throw new InvalidOperationException("Unable to determine product ID.");
    }

    private async Task<SqlConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
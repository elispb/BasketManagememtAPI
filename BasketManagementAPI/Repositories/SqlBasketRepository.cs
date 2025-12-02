using System.Data;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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
                await InsertItemAsync(connection, basket.Id, item, transaction);
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
            await DeleteItemsAsync(connection, basket.Id, transaction);
            foreach (var item in basket.Items)
            {
                await InsertItemAsync(connection, basket.Id, item, transaction);
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

    private static (byte? Type, int? Amount) GetItemDiscountData(IBasketItemDiscount? discount)
    {
        return discount switch
        {
            FlatAmountItemDiscount flat => ((byte)ItemDiscountType.FlatAmount, flat.AmountTaken),
            BuyOneGetOneFreeItemDiscount => ((byte)ItemDiscountType.Bogo, 0),
            null => (null, null),
            _ => throw new NotSupportedException("Unsupported item discount type.")
        };
    }

    private static IBasketItemDiscount? BuildItemDiscount(byte? type, int? amount)
    {
        if (!type.HasValue)
        {
            return null;
        }

        return ((ItemDiscountType)type.Value) switch
        {
            ItemDiscountType.FlatAmount => new FlatAmountItemDiscount(amount ?? 0),
            ItemDiscountType.Bogo => new BuyOneGetOneFreeItemDiscount(),
            _ => throw new NotSupportedException($"Item discount type '{type}' is not supported.")
        };
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
            var productId = reader.GetString(0);
            var name = reader.GetString(1);
            var unitPrice = reader.GetInt32(2);
            var quantity = reader.GetInt32(3);
            var discountType = reader.IsDBNull(4) ? null : (byte?)reader.GetByte(4);
            var discountAmount = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5);
            var discount = BuildItemDiscount(discountType, discountAmount);

            var item = new Item(productId, name, unitPrice, quantity, discount);
            basket.AddOrUpdateItem(item);
        }
    }

    private static async Task LoadShippingAsync(SqlConnection connection, Basket basket)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                [Country],
                [Cost]
            FROM [dbo].[BasketShipping]
            WHERE [BasketId] = @BasketId;
            """;
        command.Parameters.AddWithValue("@BasketId", basket.Id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var country = reader.GetString(0);
            var cost = reader.GetInt32(1);
            basket.SetShipping(new ShippingDetails(country, cost));
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

    private static async Task DeleteItemsAsync(SqlConnection connection, Guid basketId, SqlTransaction transaction)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "usp_DeleteItemsForBasket";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BasketId", basketId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertItemAsync(SqlConnection connection, Guid basketId, Item item, SqlTransaction transaction)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "usp_InsertItem";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Id", Guid.NewGuid());
        command.Parameters.AddWithValue("@BasketId", basketId);
        command.Parameters.AddWithValue("@ProductId", item.ProductId);
        command.Parameters.AddWithValue("@Name", item.Name);
        command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
        command.Parameters.AddWithValue("@Quantity", item.Quantity);

        var (type, amount) = GetItemDiscountData(item.ItemDiscount);
        command.Parameters.AddWithValue("@ItemDiscountType", type.HasValue ? (object)type.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ItemDiscountAmount", amount.HasValue ? (object)amount.Value : DBNull.Value);

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
        command.Parameters.AddWithValue("@Country", shipping.Country);
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

    private async Task<SqlConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
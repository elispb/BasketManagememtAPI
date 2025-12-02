using System.Text.Json;
using System.Text.Json.Serialization;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Domain.Shipping;
using Microsoft.Data.SqlClient;

namespace BasketManagementAPI.Repositories;

public sealed class SqlBasketRepository : IBasketRepository
{
    private readonly string _connectionString;
    private readonly JsonSerializerOptions _serializerOptions;

    public SqlBasketRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A connection string named 'DefaultConnection' was not found.");

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        EnsureTable();
    }

    private void EnsureTable()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            IF NOT EXISTS (
                SELECT 1
                FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[Baskets]')
                  AND type = N'U'
            )
            BEGIN
                CREATE TABLE [dbo].[Baskets] (
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [Data] NVARCHAR(MAX) NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL,
                    [ModifiedAt] DATETIME2 NOT NULL
                );
            END
            """;

        command.ExecuteNonQuery();
    }

    public async Task<Basket> GetAsync(Guid id)
    {
        using var connection = await CreateOpenConnectionAsync();
        using var command = CreateCommand(connection, "SELECT [Data] FROM [dbo].[Baskets] WHERE [Id] = @Id");
        command.Parameters.AddWithValue("@Id", id);

        var result = await command.ExecuteScalarAsync();
        if (result is not string payload)
        {
            throw new KeyNotFoundException($"Basket '{id}' not found.");
        }

        var state = JsonSerializer.Deserialize<BasketState>(payload, _serializerOptions)
            ?? throw new InvalidOperationException("Corrupted basket payload.");

        return state.ToDomain();
    }

    public async Task CreateAsync(Basket basket)
    {
        var state = BasketState.FromDomain(basket);
        var payload = JsonSerializer.Serialize(state, _serializerOptions);

        using var connection = await CreateOpenConnectionAsync();
        using var command = CreateCommand(connection, """
            INSERT INTO [dbo].[Baskets] ([Id], [Data], [CreatedAt], [ModifiedAt])
            VALUES (@Id, @Data, @CreatedAt, @ModifiedAt);
            """);
        command.Parameters.AddWithValue("@Id", basket.Id);
        command.Parameters.AddWithValue("@Data", payload);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@ModifiedAt", DateTime.UtcNow);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number == 2627)
        {
            throw new InvalidOperationException($"Basket '{basket.Id}' already exists.", ex);
        }
    }

    public async Task SaveAsync(Basket basket)
    {
        var state = BasketState.FromDomain(basket);
        var payload = JsonSerializer.Serialize(state, _serializerOptions);

        using var connection = await CreateOpenConnectionAsync();
        using var command = CreateCommand(connection, """
            UPDATE [dbo].[Baskets]
            SET [Data] = @Data, [ModifiedAt] = @ModifiedAt
            WHERE [Id] = @Id;
            """);
        command.Parameters.AddWithValue("@Id", basket.Id);
        command.Parameters.AddWithValue("@Data", payload);
        command.Parameters.AddWithValue("@ModifiedAt", DateTime.UtcNow);

        var affected = await command.ExecuteNonQueryAsync();
        if (affected == 0)
        {
            throw new KeyNotFoundException($"Basket '{basket.Id}' not found.");
        }
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string text)
    {
        var command = connection.CreateCommand();
        command.CommandText = text;
        return command;
    }

    private async Task<SqlConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private sealed record BasketState(
        Guid Id,
        List<ItemState> Items,
        BasketDiscountState? BasketDiscount,
        ShippingState? ShippingDetails)
    {
        public Basket ToDomain()
        {
            var basket = new Basket(Id);

            foreach (var itemState in Items)
            {
                basket.AddOrUpdateItem(itemState.ToDomain());
            }

            if (BasketDiscount is not null)
            {
                basket.ApplyDiscount(new PercentageBasketDiscount(BasketDiscount.Code, BasketDiscount.Percentage));
            }

            if (ShippingDetails is not null)
            {
                basket.SetShipping(new ShippingDetails(ShippingDetails.Country, ShippingDetails.Cost));
            }

            return basket;
        }

        public static BasketState FromDomain(Basket basket)
        {
            var discountState = basket.BasketDiscount switch
            {
                PercentageBasketDiscount percent => new BasketDiscountState(percent.Code, percent.Percentage),
                null => null,
                _ => throw new NotSupportedException("Unsupported basket discount type.")
            };

            return new BasketState(
                basket.Id,
                basket.Items.Select(ItemState.FromDomain).ToList(),
                discountState,
                basket.ShippingDetails is null ? null : new ShippingState(basket.ShippingDetails.Country, basket.ShippingDetails.Cost));
        }
    }

    private sealed record ItemState(
        string ProductId,
        string Name,
        int UnitPrice,
        int Quantity,
        ItemDiscountState? ItemDiscount)
    {
        public Item ToDomain()
        {
            return new Item(
                ProductId,
                Name,
                UnitPrice,
                Quantity,
                ItemDiscount?.ToDomain());
        }

        public static ItemState FromDomain(Item item)
        {
            return new ItemState(
                item.ProductId,
                item.Name,
                item.UnitPrice,
                item.Quantity,
                ItemDiscountState.FromDomain(item.ItemDiscount));
        }
    }

    private sealed record ItemDiscountState(ItemDiscountType Type, int Amount)
    {
        public IBasketItemDiscount ToDomain()
        {
            return Type switch
            {
                ItemDiscountType.FlatAmount => new FlatAmountItemDiscount(Amount),
                ItemDiscountType.Bogo => new BuyOneGetOneFreeItemDiscount(),
                _ => throw new NotSupportedException($"Item discount type '{Type}' is not supported.")
            };
        }

        public static ItemDiscountState? FromDomain(IBasketItemDiscount? discount)
        {
            return discount switch
            {
                FlatAmountItemDiscount flat => new ItemDiscountState(ItemDiscountType.FlatAmount, flat.AmountTaken),
                BuyOneGetOneFreeItemDiscount => new ItemDiscountState(ItemDiscountType.Bogo, 0),
                null => null,
                _ => throw new NotSupportedException("Unsupported item discount type.")
            };
        }
    }

    private sealed record BasketDiscountState(string Code, decimal Percentage);

    private sealed record ShippingState(string Country, int Cost);
}
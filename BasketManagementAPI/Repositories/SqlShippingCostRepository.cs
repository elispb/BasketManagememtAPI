using BasketManagementAPI.Domain.Shipping;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BasketManagementAPI.Repositories;

public sealed class SqlShippingCostRepository : IShippingCostRepository
{
    private readonly string _connectionString;

    public SqlShippingCostRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A connection string named 'DefaultConnection' was not found.");
    }

    public async Task<int?> GetCostAsync(CountryCode countryCode)
    {
        if (countryCode == CountryCode.Unknown)
        {
            return null;
        }

        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1) [Cost]
            FROM [dbo].[ShippingCosts]
            WHERE [CountryCode] = @CountryCode;
            """;
        command.Parameters.AddWithValue("@CountryCode", (int)countryCode);

        var result = await command.ExecuteScalarAsync();
        return result is int cost ? cost : null;
    }

    private async Task<SqlConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}


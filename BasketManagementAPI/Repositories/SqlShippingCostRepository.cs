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

    public async Task<int?> GetCostAsync(string country)
    {
        var normalized = (country ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1) [Cost]
            FROM [dbo].[ShippingCosts]
            WHERE UPPER([Country]) = @Country;
            """;
        command.Parameters.AddWithValue("@Country", normalized.ToUpperInvariant());

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


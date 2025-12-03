using System;
using System.Data;
using BasketManagementAPI.Domain.Discounts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BasketManagementAPI.Repositories;

public sealed class SqlDiscountDefinitionRepository : IDiscountDefinitionRepository
{
    private readonly string _connectionString;

    public SqlDiscountDefinitionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A connection string named 'DefaultConnection' was not found.");
    }

    public async Task<DiscountDefinition?> GetByCodeAsync(string code)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                [Id],
                [Code],
                [Percentage],
                [Metadata],
                [IsActive]
            FROM [dbo].[DiscountDefinitions]
            WHERE [Code] = @Code;
            """;
        command.Parameters.AddWithValue("@Code", code);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return ReadDefinition(reader);
    }

    public async Task<DiscountDefinition?> GetByIdAsync(Guid id)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                [Id],
                [Code],
                [Percentage],
                [Metadata],
                [IsActive]
            FROM [dbo].[DiscountDefinitions]
            WHERE [Id] = @Id;
            """;
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return ReadDefinition(reader);
    }

    public async Task<Guid> UpsertAsync(string code, decimal percentage)
    {
        var existing = await GetByCodeAsync(code);
        if (existing is not null)
        {
            await UpdateDefinitionAsync(existing.Id, code, percentage);
            return existing.Id;
        }

        var id = Guid.NewGuid();
        await InsertDefinitionAsync(id, code, percentage);
        return id;
    }

    private static DiscountDefinition ReadDefinition(SqlDataReader reader)
    {
        return new DiscountDefinition(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetDecimal(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetBoolean(4));
    }

    private async Task InsertDefinitionAsync(Guid id, string code, decimal percentage)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "usp_InsertDiscountDefinition";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Code", code);
        command.Parameters.AddWithValue("@Percentage", percentage);
        command.Parameters.AddWithValue("@Metadata", DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", true);

        await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateDefinitionAsync(Guid id, string code, decimal percentage)
    {
        await using var connection = await CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "usp_UpdateDiscountDefinition";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Code", code);
        command.Parameters.AddWithValue("@Percentage", percentage);
        command.Parameters.AddWithValue("@Metadata", DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", true);

        await command.ExecuteNonQueryAsync();
    }

    private async Task<SqlConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}


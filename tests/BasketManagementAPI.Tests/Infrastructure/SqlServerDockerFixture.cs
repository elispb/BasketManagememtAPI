using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace BasketManagementAPI.Tests.Infrastructure;

[CollectionDefinition("DatabaseContainer")]
public sealed class DatabaseContainerCollection : ICollectionFixture<SqlServerDockerFixture>
{
}

public sealed class SqlServerDockerFixture : IAsyncLifetime
{
    private static readonly string RepositoryRoot = LocateRepositoryRoot();
    private static readonly string ComposeFile = Path.Combine(RepositoryRoot, "docker-compose.tests.yml");
    private static readonly string DatabaseProject = Path.Combine(
        RepositoryRoot,
        "Database",
        "BasketManagementAPI.Database",
        "BasketManagementAPI.Database.csproj");
    private static readonly string Configuration =
        Environment.GetEnvironmentVariable("Configuration")
        ?? Environment.GetEnvironmentVariable("CONFIGURATION")
        ?? "Debug";

    private const string DefaultConnectionString =
        "Server=localhost,1433;Database=BasketDb;User Id=sa;Password=Str0ng!Passw0rd;TrustServerCertificate=True;";

    public async Task InitializeAsync()
    {
        await RunCommandAsync("docker", $"compose -f \"{ComposeFile}\" up --wait -d sqlserver");
        await RunCommandAsync(
            "docker",
            "exec basketmanagementapi-tests-db-integration /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Str0ng!Passw0rd -C -Q \"IF DB_ID(N'BasketDb') IS NULL CREATE DATABASE BasketDb;\"");
        await RunCommandAsync("dotnet", $"run --project \"{DatabaseProject}\" --configuration {Configuration}");
        await SeedTestDataAsync(DefaultConnectionString);
    }

    public async Task DisposeAsync()
    {
        await RunCommandAsync("docker", $"compose -f \"{ComposeFile}\" down");
    }

    public string ConnectionString => DefaultConnectionString;

    private static string LocateRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BasketManagementAPI.sln")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("Could not locate repository root.");
        }

        return directory.FullName;
    }

    private static async Task RunCommandAsync(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = RepositoryRoot,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Unable to start '{fileName}'.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Command '{fileName} {arguments}' failed with exit code {process.ExitCode}. StdOut: {output}. StdErr: {error}");
        }
    }

    private static async Task SeedTestDataAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID('dbo.DiscountDefinitions', 'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM dbo.DiscountDefinitions WHERE Code = 'TEST10')
                BEGIN
                    INSERT INTO dbo.DiscountDefinitions (Id, Code, Percentage, Metadata, IsActive, CreatedAt, ModifiedAt)
                    VALUES (NEWID(), 'TEST10', 10, '{"source":"tests","description":"Integration test discount"}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                END

                IF NOT EXISTS (SELECT 1 FROM dbo.DiscountDefinitions WHERE Code = 'TEST20')
                BEGIN
                    INSERT INTO dbo.DiscountDefinitions (Id, Code, Percentage, Metadata, IsActive, CreatedAt, ModifiedAt)
                    VALUES (NEWID(), 'TEST20', 20, '{"source":"tests","description":"Integration promo"}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                END
            END

            IF OBJECT_ID('dbo.ShippingCosts', 'U') IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM dbo.ShippingCosts WHERE Country = 'Testland')
                BEGIN
                    INSERT INTO dbo.ShippingCosts (Id, Country, CountryCode, Cost, CreatedAt, ModifiedAt)
                    VALUES (NEWID(), 'Testland', 100, 1500, SYSUTCDATETIME(), SYSUTCDATETIME());
                END

                IF NOT EXISTS (SELECT 1 FROM dbo.ShippingCosts WHERE Country = 'Sandbox Country')
                BEGIN
                    INSERT INTO dbo.ShippingCosts (Id, Country, CountryCode, Cost, CreatedAt, ModifiedAt)
                    VALUES (NEWID(), 'Sandbox Country', 101, 2500, SYSUTCDATETIME(), SYSUTCDATETIME());
                END
            END
            """;
        await command.ExecuteNonQueryAsync();
    }
}

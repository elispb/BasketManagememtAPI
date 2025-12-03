using System.Collections.Generic;
using BasketManagementAPI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BasketManagementAPI.Tests.Infrastructure;

internal sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString) => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString
            };

            config.AddInMemoryCollection(overrides);
        });
    }
}


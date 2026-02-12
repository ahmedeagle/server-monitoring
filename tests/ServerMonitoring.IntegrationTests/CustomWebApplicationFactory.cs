using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServerMonitoring.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Configures the test server with in-memory database and test-specific settings
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Ensure appsettings.Testing.json is loaded
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: false);
            
            // Add in-memory configuration for critical settings
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["UseInMemoryDatabase"] = "true",
                ["JwtSettings:SecretKey"] = "TestSecretKeyForJWT1234567890123456TestSecretKeyForJWT1234567890123456",
                ["JwtSettings:Issuer"] = "TestServerMonitoringAPI",
                ["JwtSettings:Audience"] = "TestServerMonitoringClient",
                ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Use in-memory database for all integration tests
            Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        });
    }
}

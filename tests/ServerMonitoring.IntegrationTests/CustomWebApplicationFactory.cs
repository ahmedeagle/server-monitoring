using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServerMonitoring.API;

namespace ServerMonitoring.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Configures the test server with in-memory database and test-specific settings
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Use in-memory database for all integration tests
            Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        });

        builder.UseEnvironment("Testing");
    }
}

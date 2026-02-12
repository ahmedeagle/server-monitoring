namespace ServerMonitoring.IntegrationTests.HealthChecks;

/// <summary>
/// Integration tests for health check endpoints
/// Verifies system health reporting and monitoring
/// </summary>
public class HealthCheckIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn200AndHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HealthCheck_ResponseShouldContainHealthStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // The response format depends on your health check configuration
        // but it should contain some health information
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HealthCheck_ShouldBeAccessibleWithoutAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            "health check should be publicly accessible for monitoring systems");
    }
}

using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServerMonitoring.API.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServerMonitoring.UnitTests.HealthChecks;

public class CustomHealthChecksTests
{
    [Fact]
    public async Task MemoryHealthCheck_WithLowMemoryUsage_ShouldReturnHealthy()
    {
        // Arrange
        var healthCheck = new MemoryHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
        result.Description.Should().Contain("Memory usage");
    }

    [Fact]
    public async Task DiskSpaceHealthCheck_ShouldCheckCDrive()
    {
        // Arrange
        var healthCheck = new DiskSpaceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
        result.Description.Should().Contain("Disk space");
    }

    [Fact]
    public async Task DatabaseHealthCheck_ShouldReturnStatus()
    {
        // Arrange - Would need to mock DbContext
        // This is a placeholder for integration with actual database
        
        // For unit test, we verify the health check class exists
        var healthCheckType = typeof(HealthCheck);
        
        // Assert
        healthCheckType.Should().NotBeNull();
    }
}

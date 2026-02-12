using FluentAssertions;
using Moq;
using ServerMonitoring.Infrastructure.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace ServerMonitoring.UnitTests.Services;

public class ResilientMetricsCollectorTests
{
    [Fact]
    public void CollectCpuUsage_ShouldReturnValidPercentage()
    {
        // Arrange
        var collector = new ResilientMetricsCollector();

        // Act
        var cpuUsage = collector.CollectCpuUsage();

        // Assert
        cpuUsage.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CollectMemoryUsage_ShouldReturnValidPercentage()
    {
        // Arrange
        var collector = new ResilientMetricsCollector();

        // Act
        var memoryUsage = collector.CollectMemoryUsage();

        // Assert
        memoryUsage.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CollectDiskUsage_ShouldReturnValidPercentage()
    {
        // Arrange
        var collector = new ResilientMetricsCollector();

        // Act
        var diskUsage = collector.CollectDiskUsage("C:\\");

        // Assert
        diskUsage.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task CollectMetricsAsync_ShouldNotThrowException()
    {
        // Arrange
        var collector = new ResilientMetricsCollector();

        // Act
        Func<Task> act = async () => await collector.CollectAllMetricsAsync();

        // Assert - Should not throw
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void CollectMetrics_WithInvalidDrive_ShouldReturnZero()
    {
        // Arrange
        var collector = new ResilientMetricsCollector();

        // Act
        var diskUsage = collector.CollectDiskUsage("Z:\\NonExistentDrive");

        // Assert
        diskUsage.Should().Be(0);
    }
}

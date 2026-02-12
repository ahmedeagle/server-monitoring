using FluentAssertions;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Enums;
using Xunit;

namespace ServerMonitoring.UnitTests.Domain.Entities;

public class AlertEntityTests
{
    [Fact]
    public void Alert_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var alert = new Alert();

        // Assert
        alert.Id.Should().Be(0);
        alert.ServerId.Should().Be(0);
        alert.Title.Should().Be(string.Empty);
        alert.Message.Should().Be(string.Empty);
        alert.IsAcknowledged.Should().BeFalse();
        alert.IsResolved.Should().BeFalse();
        alert.AcknowledgedAt.Should().BeNull();
        alert.ResolvedAt.Should().BeNull();
    }

    [Fact]
    public void Alert_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var alert = new Alert
        {
            ServerId = 1,
            Type = AlertType.CpuUsage,
            Severity = AlertSeverity.Warning,
            Title = "High CPU Usage",
            Message = "CPU usage exceeded 80%",
            ThresholdValue = 80.0m,
            ActualValue = 85.5m
        };

        // Assert
        alert.ServerId.Should().Be(1);
        alert.Type.Should().Be(AlertType.CpuUsage);
        alert.Severity.Should().Be(AlertSeverity.Warning);
        alert.Title.Should().Be("High CPU Usage");
        alert.Message.Should().Be("CPU usage exceeded 80%");
        alert.ThresholdValue.Should().Be(80.0m);
        alert.ActualValue.Should().Be(85.5m);
    }

    [Theory]
    [InlineData(AlertSeverity.Info)]
    [InlineData(AlertSeverity.Warning)]
    [InlineData(AlertSeverity.Error)]
    [InlineData(AlertSeverity.Critical)]
    public void Alert_ShouldAcceptAllValidSeverities(AlertSeverity severity)
    {
        // Arrange & Act
        var alert = new Alert { Severity = severity };

        // Assert
        alert.Severity.Should().Be(severity);
    }

    [Theory]
    [InlineData(AlertType.CpuUsage)]
    [InlineData(AlertType.MemoryUsage)]
    [InlineData(AlertType.DiskUsage)]
    [InlineData(AlertType.NetworkLatency)]
    public void Alert_ShouldAcceptAllValidTypes(AlertType type)
    {
        // Arrange & Act
        var alert = new Alert { Type = type };

        // Assert
        alert.Type.Should().Be(type);
    }
}

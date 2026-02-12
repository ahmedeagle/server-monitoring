using ServerMonitoring.Domain.Enums;

namespace ServerMonitoring.UnitTests.Domain.Entities;

/// <summary>
/// Unit tests for Server domain entity
/// Tests entity validation and business rules
/// </summary>
public class ServerEntityTests
{
    [Fact]
    public void Server_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var server = new Server
        {
            Id = 1,
            Name = "WebServer01",
            IpAddress = "192.168.1.100",
            Status = ServerStatus.Online,
            Description = "Production web server",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        server.Id.Should().Be(1);
        server.Name.Should().Be("WebServer01");
        server.IpAddress.Should().Be("192.168.1.100");
        server.Status.Should().Be(ServerStatus.Online);
        server.Description.Should().Be("Production web server");
        server.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Server_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var server = new Server
        {
            Name = "TestServer",
            IpAddress = "10.0.0.1"
        };

        // Assert
        server.IsDeleted.Should().BeFalse("new servers should not be deleted");
        server.Status.Should().Be(ServerStatus.Unknown, "default status should be Unknown");
    }

    [Theory]
    [InlineData(ServerStatus.Online)]
    [InlineData(ServerStatus.Offline)]
    [InlineData(ServerStatus.Warning)]
    [InlineData(ServerStatus.Critical)]
    [InlineData(ServerStatus.Unknown)]
    public void Server_Status_ShouldAcceptAllValidStatuses(ServerStatus status)
    {
        // Arrange & Act
        var server = new Server
        {
            Name = "TestServer",
            IpAddress = "192.168.1.1",
            Status = status
        };

        // Assert
        server.Status.Should().Be(status);
    }

    [Fact]
    public void Server_Metrics_NavigationProperty_ShouldBeInitializable()
    {
        // Arrange & Act
        var server = new Server
        {
            Name = "TestServer",
            IpAddress = "192.168.1.1",
            Metrics = new List<Metric>
            {
                new() { CpuUsage = 50.5, MemoryUsage = 60.0, Timestamp = DateTime.UtcNow },
                new() { CpuUsage = 55.0, MemoryUsage = 65.0, Timestamp = DateTime.UtcNow }
            }
        };

        // Assert
        server.Metrics.Should().HaveCount(2);
        server.Metrics.Should().AllBeOfType<Metric>();
    }

    [Fact]
    public void Server_Alerts_NavigationProperty_ShouldBeInitializable()
    {
        // Arrange & Act
        var server = new Server
        {
            Name = "TestServer",
            IpAddress = "192.168.1.1",
            Alerts = new List<Alert>
            {
                new() { MetricType = "CPU", Status = AlertStatus.Active },
                new() { MetricType = "Memory", Status = AlertStatus.Resolved }
            }
        };

        // Assert
        server.Alerts.Should().HaveCount(2);
        server.Alerts.Should().AllBeOfType<Alert>();
    }

    [Fact]
    public void Server_SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var server = new Server
        {
            Name = "TestServer",
            IpAddress = "192.168.1.1",
            IsDeleted = false
        };

        // Act
        server.IsDeleted = true;
        server.DeletedAt = DateTime.UtcNow;

        // Assert
        server.IsDeleted.Should().BeTrue();
        server.DeletedAt.Should().NotBeNull();
        server.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Server_AuditFields_ShouldTrackChanges()
    {
        // Arrange
        var createdTime = DateTime.UtcNow;
        var server = new Server
        {
            Name = "TestServer",
            IpAddress = "192.168.1.1",
            CreatedAt = createdTime,
            CreatedBy = "System"
        };

        // Act
        server.UpdatedAt = DateTime.UtcNow.AddMinutes(5);
        server.UpdatedBy = "AdminUser";

        // Assert
        server.CreatedAt.Should().Be(createdTime);
        server.CreatedBy.Should().Be("System");
        server.UpdatedAt.Should().BeAfter(server.CreatedAt);
        server.UpdatedBy.Should().Be("AdminUser");
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("255.255.255.255")]
    public void Server_IpAddress_ShouldAcceptValidFormats(string ipAddress)
    {
        // Arrange & Act
        var server = new Server
        {
            Name = "TestServer",
            IpAddress = ipAddress
        };

        // Assert
        server.IpAddress.Should().Be(ipAddress);
    }
}

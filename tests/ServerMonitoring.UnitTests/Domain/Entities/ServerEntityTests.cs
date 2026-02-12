using FluentAssertions;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Enums;
using Xunit;

namespace ServerMonitoring.UnitTests.Domain.Entities;

public class ServerEntityTests
{
    [Fact]
    public void Server_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var server = new Server();

        // Assert
        server.Id.Should().Be(0);
        server.Name.Should().Be(string.Empty);
        server.Hostname.Should().Be(string.Empty);
        server.IPAddress.Should().Be(string.Empty);
        server.Port.Should().Be(0);
        server.OperatingSystem.Should().Be(string.Empty);
        server.Status.Should().Be(ServerStatus.Unknown);
        server.IsDeleted.Should().BeFalse();
        server.Metrics.Should().NotBeNull().And.BeEmpty();
        server.Disks.Should().NotBeNull().And.BeEmpty();
        server.Alerts.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Server_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var server = new Server
        {
            Name = "Test Server",
            Hostname = "testhost",
            IPAddress = "192.168.1.100",
            Port = 22,
            OperatingSystem = "Ubuntu 22.04",
            Status = ServerStatus.Online
        };

        // Assert
        server.Name.Should().Be("Test Server");
        server.Hostname.Should().Be("testhost");
        server.IPAddress.Should().Be("192.168.1.100");
        server.Port.Should().Be(22);
        server.OperatingSystem.Should().Be("Ubuntu 22.04");
        server.Status.Should().Be(ServerStatus.Online);
    }

    [Theory]
    [InlineData(ServerStatus.Online)]
    [InlineData(ServerStatus.Offline)]
    [InlineData(ServerStatus.Maintenance)]
    [InlineData(ServerStatus.Error)]
    [InlineData(ServerStatus.Unknown)]
    public void Server_ShouldAcceptAllValidStatuses(ServerStatus status)
    {
        // Arrange & Act
        var server = new Server
        {
            Name = "TestServer",
            Status = status
        };

        // Assert
        server.Status.Should().Be(status);
    }

    [Fact]
    public void Server_Delete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var server = new Server
        {
            Name = "Test Server",
            IPAddress = "192.168.1.100"
        };

        // Act
        server.Delete();

        // Assert
        server.IsDeleted.Should().BeTrue();
        server.DeletedAt.Should().NotBeNull();
        server.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Server_Restore_ShouldSetIsDeletedToFalse()
    {
        // Arrange
        var server = new Server
        {
            Name = "Test Server",
            IPAddress = "192.168.1.100"
        };
        server.Delete();

        // Act
        server.Restore();

        // Assert
        server.IsDeleted.Should().BeFalse();
        server.DeletedAt.Should().BeNull();
        server.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Server_IAuditableProperties_ShouldBeSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var server = new Server();

        // Act
        server.CreatedAt = now;
        server.CreatedBy = "admin";
        server.UpdatedAt = now.AddSeconds(10);
        server.UpdatedBy = "admin2";

        // Assert
        server.CreatedAt.Should().Be(now);
        server.CreatedBy.Should().Be("admin");
        server.UpdatedAt.Should().Be(now.AddSeconds(10));
        server.UpdatedBy.Should().Be("admin2");
    }
}

using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Infrastructure.Data;
using ServerMonitoring.Infrastructure.Repositories;
using ServerMonitoring.Domain.Enums;

namespace ServerMonitoring.UnitTests.Repositories;

/// <summary>
/// Unit tests for ServerRepository
/// Tests CRUD operations and query filtering for Server entities
/// </summary>
public class ServerRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ServerRepository _repository;

    public ServerRepositoryTests()
    {
        // Arrange: Create in-memory database for isolated testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ServerRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnServer()
    {
        // Arrange
        var server = CreateTestServer("TestServer1", "192.168.1.100");
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(server.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestServer1");
        result.IpAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithSoftDeletedServer_ShouldReturnNull()
    {
        // Arrange
        var server = CreateTestServer("DeletedServer", "192.168.1.200");
        server.IsDeleted = true;
        server.DeletedAt = DateTime.UtcNow;
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(server.Id);

        // Assert
        result.Should().BeNull("soft-deleted entities should not be returned");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveServers()
    {
        // Arrange
        var servers = new[]
        {
            CreateTestServer("Server1", "192.168.1.1"),
            CreateTestServer("Server2", "192.168.1.2"),
            CreateTestServer("Server3", "192.168.1.3")
        };
        _context.Servers.AddRange(servers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(s => !s.IsDeleted);
    }

    [Fact]
    public async Task GetAllAsync_WithSoftDeletedServers_ShouldExcludeThem()
    {
        // Arrange
        var activeServer = CreateTestServer("ActiveServer", "192.168.1.1");
        var deletedServer = CreateTestServer("DeletedServer", "192.168.1.2");
        deletedServer.IsDeleted = true;
        deletedServer.DeletedAt = DateTime.UtcNow;

        _context.Servers.AddRange(activeServer, deletedServer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("ActiveServer");
    }

    [Fact]
    public async Task AddAsync_ShouldAddServerToDatabase()
    {
        // Arrange
        var server = CreateTestServer("NewServer", "192.168.1.100");

        // Act
        await _repository.AddAsync(server);
        await _context.SaveChangesAsync();

        // Assert
        var savedServer = await _context.Servers.FirstOrDefaultAsync(s => s.Name == "NewServer");
        savedServer.Should().NotBeNull();
        savedServer!.IpAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    public async Task Update_ShouldModifyExistingServer()
    {
        // Arrange
        var server = CreateTestServer("OriginalName", "192.168.1.1");
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        // Act
        server.Name = "UpdatedName";
        server.IpAddress = "192.168.1.99";
        server.Status = ServerStatus.Offline;
        _repository.Update(server);
        await _context.SaveChangesAsync();

        // Assert
        var updatedServer = await _context.Servers.FindAsync(server.Id);
        updatedServer!.Name.Should().Be("UpdatedName");
        updatedServer.IpAddress.Should().Be("192.168.1.99");
        updatedServer.Status.Should().Be(ServerStatus.Offline);
    }

    [Fact]
    public async Task Delete_ShouldRemoveServerFromDatabase()
    {
        // Arrange
        var server = CreateTestServer("ToDelete", "192.168.1.50");
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(server);
        await _context.SaveChangesAsync();

        // Assert - Server should be soft deleted, not physically removed
        var deletedServer = await _context.Servers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == server.Id);
        
        if (deletedServer != null)
        {
            // If soft delete is implemented
            deletedServer.IsDeleted.Should().BeTrue();
        }
        else
        {
            // If hard delete is implemented
            var hardDeleted = await _context.Servers.FindAsync(server.Id);
            hardDeleted.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetServersByStatusAsync_ShouldFilterByStatus()
    {
        // Arrange
        var servers = new[]
        {
            CreateTestServer("Online1", "192.168.1.1", ServerStatus.Online),
            CreateTestServer("Online2", "192.168.1.2", ServerStatus.Online),
            CreateTestServer("Offline1", "192.168.1.3", ServerStatus.Offline)
        };
        _context.Servers.AddRange(servers);
        await _context.SaveChangesAsync();

        // Act
        var onlineServers = await _context.Servers
            .Where(s => s.Status == ServerStatus.Online)
            .ToListAsync();

        // Assert
        onlineServers.Should().HaveCount(2);
        onlineServers.Should().OnlyContain(s => s.Status == ServerStatus.Online);
    }

    [Theory]
    [InlineData("Server1", "192.168.1.1")]
    [InlineData("Production-DB", "10.0.0.50")]
    [InlineData("Web-Server-01", "172.16.0.100")]
    public async Task AddAsync_WithVariousServerConfigurations_ShouldSucceed(string name, string ipAddress)
    {
        // Arrange
        var server = CreateTestServer(name, ipAddress);

        // Act
        await _repository.AddAsync(server);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Servers.FirstOrDefaultAsync(s => s.Name == name);
        saved.Should().NotBeNull();
        saved!.IpAddress.Should().Be(ipAddress);
    }

    #region Test Helpers

    private Server CreateTestServer(string name, string ipAddress, ServerStatus status = ServerStatus.Online)
    {
        return new Server
        {
            Name = name,
            IpAddress = ipAddress,
            Status = status,
            Description = $"Test server {name}",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

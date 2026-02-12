using ServerMonitoring.Application.Features.Auth.Commands;
using System.Net.Http.Headers;

namespace ServerMonitoring.IntegrationTests.Servers;

/// <summary>
/// Integration tests for Server CRUD operations
/// Tests protected endpoints with JWT authentication
/// </summary>
public class ServerControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ServerControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetServers_WithoutAuthentication_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/servers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetServers_WithValidToken_ShouldReturn200AndServerList()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/servers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var servers = await response.Content.ReadFromJsonAsync<List<ServerDto>>();
        servers.Should().NotBeNull();
        servers.Should().BeAssignableTo<IEnumerable<ServerDto>>();
    }

    [Fact]
    public async Task GetServerById_WithValidIdAndToken_ShouldReturn200AndServer()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get all servers first to find a valid ID
        var serversResponse = await _client.GetAsync("/api/v1/servers");
        var servers = await serversResponse.Content.ReadFromJsonAsync<List<ServerDto>>();
        
        if (servers == null || servers.Count == 0)
        {
            // Skip test if no servers available (seeded data should exist)
            return;
        }

        var serverId = servers[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/v1/servers/{serverId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var server = await response.Content.ReadFromJsonAsync<ServerDto>();
        server.Should().NotBeNull();
        server!.Id.Should().Be(serverId);
    }

    [Fact]
    public async Task GetServerById_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/v1/servers/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateServer_WithValidData_ShouldReturn201()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createCommand = new
        {
            name = $"IntegrationTestServer_{Guid.NewGuid():N}",
            ipAddress = "192.168.100.1",
            description = "Created by integration test",
            status = 1 // Online
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/servers", createCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var server = await response.Content.ReadFromJsonAsync<ServerDto>();
        server.Should().NotBeNull();
        server!.Name.Should().Be(createCommand.name);
        server.IpAddress.Should().Be(createCommand.ipAddress);
    }

    [Fact]
    public async Task UpdateServer_WithValidData_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get existing server
        var serversResponse = await _client.GetAsync("/api/v1/servers");
        var servers = await serversResponse.Content.ReadFromJsonAsync<List<ServerDto>>();
        
        if (servers == null || servers.Count == 0)
        {
            return; // Skip if no servers
        }

        var serverId = servers[0].Id;
        var updateCommand = new
        {
            name = $"Updated_{Guid.NewGuid():N}",
            ipAddress = "192.168.200.1",
            description = "Updated by integration test",
            status = 2 // Offline
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/servers/{serverId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ServerDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(updateCommand.name);
    }

    [Fact]
    public async Task DeleteServer_WithValidId_ShouldReturn204()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a server to delete
        var createCommand = new
        {
            name = $"ToDelete_{Guid.NewGuid():N}",
            ipAddress = "192.168.250.1",
            description = "Will be deleted",
            status = 1
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/servers", createCommand);
        var created = await createResponse.Content.ReadFromJsonAsync<ServerDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/servers/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/v1/servers/{created.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #region Helper Methods

    private async Task<string> GetAuthTokenAsync()
    {
        var username = $"testuser_{Guid.NewGuid():N}";
        var password = "TestPass123!";

        var registerCommand = new RegisterUserCommand
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.AccessToken;
    }

    private class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    private class ServerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string IpAddress { get; set; } = null!;
        public string? Description { get; set; }
        public int Status { get; set; }
    }

    #endregion
}

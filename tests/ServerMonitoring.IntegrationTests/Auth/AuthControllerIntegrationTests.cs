using ServerMonitoring.Application.Features.Auth.Commands;
using System.Net.Http.Headers;

namespace ServerMonitoring.IntegrationTests.Auth;

/// <summary>
/// Integration tests for Authentication endpoints
/// Tests the complete authentication flow from HTTP request to database
/// </summary>
public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200AndTokens()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            Username = $"integrationtest_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "SecurePass123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be(registerCommand.Username);
        result.User.Email.Should().Be(registerCommand.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldReturn400()
    {
        // Arrange
        var username = $"duplicate_{Guid.NewGuid():N}";
        var firstCommand = new RegisterUserCommand
        {
            Username = username,
            Email = $"first_{Guid.NewGuid():N}@example.com",
            Password = "SecurePass123!"
        };

        var secondCommand = new RegisterUserCommand
        {
            Username = username,
            Email = $"second_{Guid.NewGuid():N}@example.com",
            Password = "SecurePass123!"
        };

        // Act
        await _client.PostAsJsonAsync("/api/v1/auth/register", firstCommand);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", secondCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Username = $"weakpass_{Guid.NewGuid():N}",
            Email = $"weak_{Guid.NewGuid():N}@example.com",
            Password = "weak"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("password", "error message should mention password validation");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200AndTokens()
    {
        // Arrange - First register a user
        var username = $"logintest_{Guid.NewGuid():N}";
        var password = "LoginPass123!";
        var registerCommand = new RegisterUserCommand
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = password
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);

        var loginCommand = new LoginCommand
        {
            Username = username,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Username.Should().Be(username);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange - Register user
        var username = $"wrongpass_{Guid.NewGuid():N}";
        var registerCommand = new RegisterUserCommand
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = "CorrectPass123!"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);

        var loginCommand = new LoginCommand
        {
            Username = username,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturn401()
    {
        // Arrange
        var loginCommand = new LoginCommand
        {
            Username = "nonexistent_user_12345",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturn200AndNewTokens()
    {
        // Arrange - Register and get tokens
        var username = $"refresh_{Guid.NewGuid():N}";
        var registerCommand = new RegisterUserCommand
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = "RefreshPass123!"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshCommand = new
        {
            refreshToken = authResult!.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", refreshCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().NotBe(authResult.AccessToken, "should generate new access token");
    }

    #region Helper Classes

    private class AuthResponse
    {
        public UserDto User { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    private class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    #endregion
}

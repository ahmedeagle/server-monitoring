using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Features.Auth.Commands;
using ServerMonitoring.Infrastructure.Data;
using ServerMonitoring.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace ServerMonitoring.UnitTests.Features.Auth;

/// <summary>
/// Unit tests for LoginCommandHandler
/// Tests authentication and token generation logic
/// </summary>
public class LoginCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        // Arrange: Setup in-memory database and auth service
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"JwtSettings:SecretKey", "YourSuperSecretKeyForJWT_MustBeAtLeast32Characters_ChangeInProduction!"},
                {"JwtSettings:Issuer", "ServerMonitoringAPI"},
                {"JwtSettings:Audience", "ServerMonitoringClient"},
                {"JwtSettings:AccessTokenExpirationMinutes", "60"},
                {"JwtSettings:RefreshTokenExpirationDays", "7"}
            }!)
            .Build();

        _authService = new AuthService(configuration);
        _handler = new LoginCommandHandler(_context, _authService);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var password = "ValidPass123!";
        var user = CreateTestUser("testuser", "test@example.com", password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand
        {
            Username = "testuser",
            Password = password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be("testuser");
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidUsername_ShouldThrowException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "nonexistent",
            Password = "Password123!"
        };

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var user = CreateTestUser("validuser", "valid@example.com", "CorrectPass123!");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand
        {
            Username = "validuser",
            Password = "WrongPassword123!"
        };

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task Handle_ShouldUpdateRefreshToken()
    {
        // Arrange
        var password = "Password123!";
        var user = CreateTestUser("updatetest", "update@example.com", password);
        var oldRefreshToken = "old-refresh-token";
        user.RefreshToken = oldRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand
        {
            Username = "updatetest",
            Password = password
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "updatetest");
        updatedUser.Should().NotBeNull();
        updatedUser!.RefreshToken.Should().NotBe(oldRefreshToken, "refresh token should be updated on login");
        updatedUser.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow.AddDays(6));
    }

    [Fact]
    public async Task Handle_WithUserHavingRoles_ShouldIncludeRolesInToken()
    {
        // Arrange
        var password = "AdminPass123!";
        var user = CreateTestUser("admin", "admin@example.com", password);
        
        var adminRole = new Role { Id = 1, Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow };
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync();

        user.UserRoles = new List<UserRole>
        {
            new() { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole, User = user }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand
        {
            Username = "admin",
            Password = password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        // Token should contain role claims (would need JWT decoding to verify fully)
        result.User.UserRoles.Should().HaveCount(1);
        result.User.UserRoles.First().Role.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_ShouldLoadUserRolesEagerly()
    {
        // Arrange
        var password = "Password123!";
        var user = CreateTestUser("roletest", "role@example.com", password);
        
        var userRole = new Role { Id = 2, Name = "User", Description = "Standard User", CreatedAt = DateTime.UtcNow };
        _context.Roles.Add(userRole);
        await _context.SaveChangesAsync();

        user.UserRoles = new List<UserRole>
        {
            new() { UserId = user.Id, RoleId = userRole.Id, Role = userRole, User = user }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand
        {
            Username = "roletest",
            Password = password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.User.UserRoles.Should().NotBeNull();
        result.User.UserRoles.Should().HaveCount(1);
        result.User.UserRoles.First().Role.Should().NotBeNull("roles should be loaded eagerly");
    }

    [Theory]
    [InlineData("user1", "Pass123!")]
    [InlineData("admin_user", "AdminPass123!")]
    [InlineData("test.user", "TestPass123!")]
    public async Task Handle_WithVariousValidCredentials_ShouldSucceed(string username, string password)
    {
        // Arrange
        var user = CreateTestUser(username, $"{username}@example.com", password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand
        {
            Username = username,
            Password = password
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Username.Should().Be(username);
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    #region Test Helpers

    private User CreateTestUser(string username, string email, string password)
    {
        var hashedPassword = _authService.HashPassword(password);
        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole>()
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

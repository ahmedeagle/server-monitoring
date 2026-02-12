using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Features.Auth.Commands;
using ServerMonitoring.Infrastructure.Data;
using ServerMonitoring.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace ServerMonitoring.UnitTests.Features.Auth;

/// <summary>
/// Unit tests for RegisterUserCommandHandler
/// Tests user registration business logic and validation
/// </summary>
public class RegisterUserCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        // Arrange: Setup in-memory database and dependencies
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
        _handler = new RegisterUserCommandHandler(_context, _authService);

        // Seed default "User" role
        SeedDefaultRole();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUser()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "SecurePass123!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be("newuser");
        result.User.Email.Should().Be("newuser@example.com");
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldHashPassword()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        user.Should().NotBeNull();
        user!.PasswordHash.Should().NotBe("Password123!", "password should be hashed");
        user.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateUsername_ShouldThrowException()
    {
        // Arrange
        var existingUser = new User
        {
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var command = new RegisterUserCommand
        {
            Username = "existinguser",
            Email = "different@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*username*already exists*");
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var existingUser = new User
        {
            Username = "user1",
            Email = "duplicate@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var command = new RegisterUserCommand
        {
            Username = "user2",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*already exists*");
    }

    [Fact]
    public async Task Handle_ShouldAssignDefaultUserRole()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Username = "roletest",
            Email = "roletest@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == "roletest");

        user.Should().NotBeNull();
        user!.UserRoles.Should().HaveCount(1);
        user.UserRoles.First().Role.Name.Should().Be("User");
    }

    [Fact]
    public async Task Handle_ShouldSetRefreshTokenWithExpiration()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Username = "tokentest",
            Email = "tokentest@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "tokentest");
        user.Should().NotBeNull();
        user!.RefreshToken.Should().NotBeNullOrEmpty();
        user.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);
        user.RefreshTokenExpiry.Should().BeOnOrBefore(DateTime.UtcNow.AddDays(8));
    }

    [Theory]
    [InlineData("user1", "user1@example.com")]
    [InlineData("admin_user", "admin@company.com")]
    [InlineData("test.user", "test.user@domain.org")]
    public async Task Handle_WithVariousValidInputs_ShouldSucceed(string username, string email)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Username = username,
            Email = email,
            Password = "ValidPass123!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Username.Should().Be(username);
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_ShouldPersistUserToDatabase()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Username = "persisttest",
            Email = "persist@example.com",
            Password = "Password123!"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var userCount = await _context.Users.CountAsync(u => u.Username == "persisttest");
        userCount.Should().Be(1, "user should be persisted exactly once");
    }

    #region Test Helpers

    private void SeedDefaultRole()
    {
        var userRole = new Role
        {
            Id = 2,
            Name = "User",
            Description = "Standard User Role",
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(userRole);
        _context.SaveChanges();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

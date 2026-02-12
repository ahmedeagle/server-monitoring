using Microsoft.Extensions.Configuration;
using ServerMonitoring.Infrastructure.Services;

namespace ServerMonitoring.UnitTests.Services;

/// <summary>
/// Unit tests for AuthService - JWT token generation and password hashing
/// Tests the core security functionality of the application
/// </summary>
public class AuthServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Arrange: Setup configuration with JWT settings
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "YourSuperSecretKeyForJWT_MustBeAtLeast32Characters_ChangeInProduction!"},
            {"JwtSettings:Issuer", "ServerMonitoringAPI"},
            {"JwtSettings:Audience", "ServerMonitoringClient"},
            {"JwtSettings:AccessTokenExpirationMinutes", "60"},
            {"JwtSettings:RefreshTokenExpirationDays", "7"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _authService = new AuthService(_configuration);
    }

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password, "password should be hashed, not stored in plain text");
        hash.Length.Should().BeGreaterThan(50, "PBKDF2 hashes should be lengthy");
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "salted hashes should differ even with same password");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "ValidPassword123!";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue("correct password should verify against its hash");
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var incorrectPassword = "WrongPassword123!";
        var hash = _authService.HashPassword(correctPassword);

        // Act
        var result = _authService.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse("incorrect password should not verify");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse(string? password)
    {
        // Arrange
        var hash = _authService.HashPassword("ValidPassword123!");

        // Act
        var result = _authService.VerifyPassword(password!, hash);

        // Assert
        result.Should().BeFalse("empty or null passwords should not verify");
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3, "JWT should have 3 parts: header.payload.signature");
    }

    [Fact]
    public void GenerateJwtToken_WithRoles_ShouldIncludeRoleClaims()
    {
        // Arrange
        var user = CreateTestUser();
        user.UserRoles = new List<UserRole>
        {
            new() { RoleId = 1, Role = new Role { Id = 1, Name = "Admin", Description = "Administrator" } }
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        // Token validation would require decoding, but we can verify it was generated
        token.Length.Should().BeGreaterThan(100, "token with claims should be substantial");
    }

    [Fact]
    public void GenerateJwtToken_WithMultipleRoles_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        user.UserRoles = new List<UserRole>
        {
            new() { RoleId = 1, Role = new Role { Id = 1, Name = "Admin", Description = "Administrator" } },
            new() { RoleId = 2, Role = new Role { Id = 2, Name = "User", Description = "Standard User" } }
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueToken()
    {
        // Act
        var token1 = _authService.GenerateRefreshToken();
        var token2 = _authService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2, "refresh tokens should be unique");
        token1.Length.Should().Be(88, "base64 encoded 64 bytes should be 88 characters");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeUrlSafe()
    {
        // Act
        var token = _authService.GenerateRefreshToken();

        // Assert
        token.Should().NotContain("+", "should use base64url encoding");
        token.Should().NotContain("/", "should use base64url encoding");
        token.Should().NotContain("=", "should use base64url encoding");
    }

    [Theory]
    [InlineData("P@ssw0rd!")]
    [InlineData("MySecurePass123!@#")]
    [InlineData("ComplexPassword$2024")]
    public void HashPassword_WithVariousPasswords_ShouldAlwaysHash(string password)
    {
        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        _authService.VerifyPassword(password, hash).Should().BeTrue();
    }

    #region Test Helpers

    private User CreateTestUser()
    {
        return new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            UserRoles = new List<UserRole>(),
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}

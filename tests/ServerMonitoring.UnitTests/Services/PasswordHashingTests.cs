using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Configuration;
using ServerMonitoring.Infrastructure.Services;
using Moq;

namespace ServerMonitoring.UnitTests.Services;

public class PasswordHashingTests
{
    private readonly AuthService _authService;

    public PasswordHashingTests()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["Jwt:SecretKey"]).Returns("TestSecretKeyForJWT1234567890123456");
        mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        mockConfig.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
        mockConfig.Setup(x => x["Jwt:ExpiryInMinutes"]).Returns("60");
        
        _authService = new AuthService(mockConfig.Object);
    }

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyHash()
    {
        // Arrange
        var password = "Test@Password123";

        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_ShouldProduceDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "Test@Password123";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "salts should make hashes different");
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var password = "Test@Password123";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var password = "Test@Password123";
        var wrongPassword = "WrongPassword456";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        // Arrange
        int userId = 1;
        string username = "testuser";
        string email = "test@example.com";
        var roles = new List<string> { "Admin", "User" };

        // Act
        var token = _authService.GenerateJwtToken(userId, username, email, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Length.Should().Be(3, "JWT should have 3 parts");
    }
}

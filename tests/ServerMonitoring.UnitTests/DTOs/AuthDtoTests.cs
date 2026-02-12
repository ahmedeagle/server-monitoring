using FluentAssertions;
using ServerMonitoring.Application.DTOs;
using Xunit;

namespace ServerMonitoring.UnitTests.DTOs;

public class AuthDtoTests
{
    [Fact]
    public void RegisterUserDto_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var dto = new RegisterUserDto();

        // Assert
        dto.Username.Should().Be(string.Empty);
        dto.Email.Should().Be(string.Empty);
        dto.Password.Should().Be(string.Empty);
        dto.FirstName.Should().Be(string.Empty);
        dto.LastName.Should().Be(string.Empty);
    }

    [Fact]
    public void RegisterUserDto_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var dto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test@123",
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        dto.Username.Should().Be("testuser");
        dto.Email.Should().Be("test@example.com");
        dto.Password.Should().Be("Test@123");
        dto.FirstName.Should().Be("Test");
        dto.LastName.Should().Be("User");
    }

    [Fact]
    public void LoginDto_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var dto = new LoginDto
        {
            Username = "testuser",
            Password = "Test@123"
        };

        // Assert
        dto.Username.Should().Be("testuser");
        dto.Password.Should().Be("Test@123");
    }

    [Fact]
    public void AuthResponseDto_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var roles = new List<string> { "Admin" };

        // Act
        var dto = new AuthResponseDto
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Token = "jwt-token-here",
            RefreshToken = "refresh-token-here",
            ExpiresAt = expiresAt,
            Roles = roles
        };

        // Assert
        dto.UserId.Should().Be(1);
        dto.Username.Should().Be("testuser");
        dto.Email.Should().Be("test@example.com");
        dto.Token.Should().Be("jwt-token-here");
        dto.RefreshToken.Should().Be("refresh-token-here");
        dto.ExpiresAt.Should().Be(expiresAt);
        dto.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void UserDto_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var lastLogin = DateTime.UtcNow;
        var roles = new List<string> { "Admin", "User" };

        // Act
        var dto = new UserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            LastLoginDate = lastLogin,
            Roles = roles
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Username.Should().Be("testuser");
        dto.Email.Should().Be("test@example.com");
        dto.FirstName.Should().Be("Test");
        dto.LastName.Should().Be("User");
        dto.IsActive.Should().BeTrue();
        dto.LastLoginDate.Should().Be(lastLogin);
        dto.Roles.Should().BeEquivalentTo(roles);
    }
}

namespace ServerMonitoring.UnitTests.Domain.Entities;

/// <summary>
/// Unit tests for User domain entity
/// Tests user entity properties and relationships
/// </summary>
public class UserEntityTests
{
    [Fact]
    public void User_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        user.Id.Should().Be(1);
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashed_password");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_RefreshToken_ShouldBeNullableAndUpdatable()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed"
        };

        // Act
        user.RefreshToken = "new-refresh-token";
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Assert
        user.RefreshToken.Should().Be("new-refresh-token");
        user.RefreshTokenExpiry.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void User_UserRoles_NavigationProperty_ShouldBeInitializable()
    {
        // Arrange & Act
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed",
            UserRoles = new List<UserRole>
            {
                new() { RoleId = 1, Role = new Role { Id = 1, Name = "Admin", Description = "Admin" } },
                new() { RoleId = 2, Role = new Role { Id = 2, Name = "User", Description = "User" } }
            }
        };

        // Assert
        user.UserRoles.Should().HaveCount(2);
        user.UserRoles.Should().AllBeOfType<UserRole>();
    }

    [Fact]
    public void User_SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed",
            IsDeleted = false
        };

        // Act
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("admin", "admin@company.com")]
    [InlineData("user123", "user123@domain.org")]
    [InlineData("test.user", "test.user@example.com")]
    public void User_WithVariousCredentials_ShouldBeValid(string username, string email)
    {
        // Arrange & Act
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = "hashed_password"
        };

        // Assert
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
    }

    [Fact]
    public void User_AuditFields_ShouldTrackChanges()
    {
        // Arrange
        var createdTime = DateTime.UtcNow;
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed",
            CreatedAt = createdTime,
            CreatedBy = "System"
        };

        // Act
        user.UpdatedAt = DateTime.UtcNow.AddMinutes(10);
        user.UpdatedBy = "AdminUser";

        // Assert
        user.CreatedAt.Should().Be(createdTime);
        user.CreatedBy.Should().Be("System");
        user.UpdatedAt.Should().BeAfter(user.CreatedAt);
        user.UpdatedBy.Should().Be("AdminUser");
    }

    [Fact]
    public void User_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var user = new User
        {
            Username = "newuser",
            Email = "new@example.com",
            PasswordHash = "hashed"
        };

        // Assert
        user.IsDeleted.Should().BeFalse("new users should not be deleted");
        user.RefreshToken.Should().BeNull("refresh token should be null initially");
        user.RefreshTokenExpiry.Should().BeNull("refresh token expiry should be null initially");
    }
}

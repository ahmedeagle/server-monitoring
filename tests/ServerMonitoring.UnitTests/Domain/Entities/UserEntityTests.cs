using FluentAssertions;
using ServerMonitoring.Domain.Entities;
using Xunit;

namespace ServerMonitoring.UnitTests.Domain.Entities;

public class UserEntityTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Username.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.PasswordHash.Should().Be(string.Empty);
        user.FirstName.Should().Be(string.Empty);
        user.LastName.Should().Be(string.Empty);
        user.IsActive.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
        user.LastLoginDate.Should().BeNull();
        user.UserRoles.Should().NotBeNull().And.BeEmpty();
        user.Alerts.Should().NotBeNull().And.BeEmpty();
        user.Reports.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void User_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        // Assert
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_Delete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        user.Delete();

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void User_Restore_ShouldSetIsDeletedToFalse()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        user.Delete();

        // Act
        user.Restore();

        // Assert
        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
        user.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void User_IAuditableProperties_ShouldBeSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var user = new User();

        // Act
        user.CreatedAt = now;
        user.CreatedBy = "system";
        user.UpdatedAt = now.AddSeconds(10);
        user.UpdatedBy = "admin";

        // Assert
        user.CreatedAt.Should().Be(now);
        user.CreatedBy.Should().Be("system");
        user.UpdatedAt.Should().Be(now.AddSeconds(10));
        user.UpdatedBy.Should().Be("admin");
    }
}

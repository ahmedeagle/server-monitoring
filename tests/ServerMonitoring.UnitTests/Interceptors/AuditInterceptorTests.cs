using FluentAssertions;
using ServerMonitoring.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServerMonitoring.UnitTests.Interceptors;

public class AuditInterceptorTests
{
    [Fact]
    public async Task SavingChanges_WithNewEntity_ShouldSetCreatedAt()
    {
        // Arrange
        var interceptor = new AuditInterceptor();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        using var context = new TestDbContext(options);
        var server = new Server { Name = "Test Server", IpAddress = "192.168.1.1" };

        // Act
        context.Servers.Add(server);
        await context.SaveChangesAsync();

        // Assert
        server.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        server.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SavingChanges_WithModifiedEntity_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var interceptor = new AuditInterceptor();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        using var context = new TestDbContext(options);
        var server = new Server { Name = "Test Server", IpAddress = "192.168.1.1" };
        context.Servers.Add(server);
        await context.SaveChangesAsync();

        var originalCreatedAt = server.CreatedAt;
        var originalUpdatedAt = server.UpdatedAt;

        await Task.Delay(100); // Small delay to ensure time difference

        // Act
        server.Name = "Updated Server";
        context.Servers.Update(server);
        await context.SaveChangesAsync();

        // Assert
        server.CreatedAt.Should().Be(originalCreatedAt); // Should not change
        server.UpdatedAt.Should().BeAfter(originalUpdatedAt); // Should be updated
    }

    [Fact]
    public async Task SavingChanges_WithDeletedEntity_ShouldSetDeletedAt()
    {
        // Arrange
        var interceptor = new AuditInterceptor();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        using var context = new TestDbContext(options);
        var server = new Server { Name = "Test Server", IpAddress = "192.168.1.1" };
        context.Servers.Add(server);
        await context.SaveChangesAsync();

        // Act
        context.Servers.Remove(server);
        await context.SaveChangesAsync();

        // Assert
        server.IsDeleted.Should().BeTrue();
        server.DeletedAt.Should().NotBeNull();
        server.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // Test DbContext for interceptor testing
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<Server> Servers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Server>(entity =>
            {
                entity.HasKey(e => e.ServerId);
                entity.Property(e => e.Name).IsRequired();
            });
        }
    }
}

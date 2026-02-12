using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ServerMonitoring.API.Middleware;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ServerMonitoring.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithoutCorrelationId_ShouldGenerateNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(next: (innerHttpContext) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        context.Response.Headers["X-Correlation-ID"].ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationId_ShouldUseProvided()
    {
        // Arrange
        var correlationId = "test-correlation-id-12345";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = correlationId;
        var middleware = new CorrelationIdMiddleware(next: (innerHttpContext) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(next: (innerHttpContext) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetItemsCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(next: (innerHttpContext) =>
        {
            // Verify correlation ID is in Items during next middleware
            innerHttpContext.Items.Should().ContainKey("CorrelationId");
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CorrelationId");
    }
}

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ServerMonitoring.API.Middleware;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ServerMonitoring.UnitTests.Middleware;

public class IdempotencyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithIdempotencyKey_ShouldProcessOnce()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers["Idempotency-Key"] = idempotencyKey;
        context.Request.Method = "POST";
        
        var executionCount = 0;
        var middleware = new IdempotencyMiddleware(next: (innerHttpContext) =>
        {
            executionCount++;
            return Task.CompletedTask;
        });

        // Act - First execution
        await middleware.InvokeAsync(context);

        // Assert
        executionCount.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_WithoutIdempotencyKey_ShouldAlwaysProcess()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        
        var executionCount = 0;
        var middleware = new IdempotencyMiddleware(next: (innerHttpContext) =>
        {
            executionCount++;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        // Assert
        executionCount.Should().Be(2);
    }

    [Fact]
    public async Task InvokeAsync_GetRequest_ShouldBypassIdempotency()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers["Idempotency-Key"] = idempotencyKey;
        context.Request.Method = "GET";  // GET requests don't use idempotency
        
        var executionCount = 0;
        var middleware = new IdempotencyMiddleware(next: (innerHttpContext) =>
        {
            executionCount++;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);
        await middleware.InvokeAsync(context);

        // Assert - Should execute twice since GET bypasses idempotency
        executionCount.Should().Be(2);
    }
}

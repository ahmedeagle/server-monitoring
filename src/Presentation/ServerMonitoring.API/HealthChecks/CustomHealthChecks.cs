using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServerMonitoring.Infrastructure.Data;
using System.Text.Json;

namespace ServerMonitoring.API.HealthChecks;

/// <summary>
/// Custom health check for database connectivity
/// Implements deep health probing
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check if database can be connected
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Execute a simple query
            var serverCount = await dbContext.Servers.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "ServerCount", serverCount },
                { "DatabaseProvider", dbContext.Database.ProviderName ?? "Unknown" },
                { "LastChecked", DateTime.UtcNow }
            };

            _logger.LogInformation("Database health check passed. Servers: {Count}", serverCount);

            return HealthCheckResult.Healthy("Database is responsive", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

/// <summary>
/// Custom health check for Redis cache
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(
        IDistributedCache cache,
        ILogger<RedisHealthCheck> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = "health_check_key";
            var value = DateTime.UtcNow.ToString("O");

            // Try to set and get a value
            await _cache.SetStringAsync(key, value, cancellationToken);
            var retrieved = await _cache.GetStringAsync(key, cancellationToken);

            if (retrieved == value)
            {
                await _cache.RemoveAsync(key, cancellationToken);
                
                _logger.LogInformation("Redis health check passed");
                
                return HealthCheckResult.Healthy("Redis is responsive", new Dictionary<string, object>
                {
                    { "LastChecked", DateTime.UtcNow }
                });
            }

            return HealthCheckResult.Degraded("Redis returned unexpected value");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis health check failed - degraded mode");
            return HealthCheckResult.Degraded("Redis unavailable, using memory cache", ex);
        }
    }
}

/// <summary>
/// Custom health check response writer with detailed JSON output
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}

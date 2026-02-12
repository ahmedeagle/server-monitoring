using System.Collections.Concurrent;

namespace ServerMonitoring.API.Middleware;

/// <summary>
/// Rate limiting middleware to prevent API abuse
/// Implements sliding window rate limiting per IP address
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RequestInfo> _clients = new();
    private const int MaxRequestsPerMinute = 100;
    private const int TimeWindowSeconds = 60;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitAttribute = endpoint?.Metadata.GetMetadata<DisableRateLimitAttribute>();
        
        // Skip rate limiting if endpoint has DisableRateLimit attribute
        if (rateLimitAttribute != null)
        {
            await _next(context);
            return;
        }

        var ipAddress = GetClientIpAddress(context);
        var currentTime = DateTime.UtcNow;

        // Get or create request info for this IP
        var requestInfo = _clients.GetOrAdd(ipAddress, _ => new RequestInfo
        {
            FirstRequestTime = currentTime,
            RequestCount = 0
        });

        bool shouldReject = false;
        
        lock (requestInfo)
        {
            // Calculate time window
            var timeSinceFirstRequest = (currentTime - requestInfo.FirstRequestTime).TotalSeconds;

            if (timeSinceFirstRequest > TimeWindowSeconds)
            {
                // Reset window
                requestInfo.FirstRequestTime = currentTime;
                requestInfo.RequestCount = 1;
            }
            else
            {
                requestInfo.RequestCount++;

                if (requestInfo.RequestCount > MaxRequestsPerMinute)
                {
                    shouldReject = true;
                }
            }
        }

        if (shouldReject)
        {
            _logger.LogWarning(
                "Rate limit exceeded for IP: {IpAddress}. Requests: {RequestCount}",
                ipAddress,
                requestInfo.RequestCount);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = TimeWindowSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                message = $"Maximum {MaxRequestsPerMinute} requests per minute allowed",
                retryAfter = $"{TimeWindowSeconds} seconds"
            });
            return;
        }

        // Cleanup old entries every 100 requests
        if (_clients.Count > 1000)
        {
            CleanupOldEntries();
        }

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        }

        // Check for real IP
        if (context.Request.Headers.ContainsKey("X-Real-IP"))
        {
            return context.Request.Headers["X-Real-IP"].ToString();
        }

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void CleanupOldEntries()
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5);
        var keysToRemove = _clients
            .Where(kvp => kvp.Value.FirstRequestTime < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _clients.TryRemove(key, out _);
        }

        _logger.LogDebug("Cleaned up {Count} old rate limit entries", keysToRemove.Count);
    }

    private class RequestInfo
    {
        public DateTime FirstRequestTime { get; set; }
        public int RequestCount { get; set; }
    }
}

/// <summary>
/// Attribute to disable rate limiting on specific endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableRateLimitAttribute : Attribute
{
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}

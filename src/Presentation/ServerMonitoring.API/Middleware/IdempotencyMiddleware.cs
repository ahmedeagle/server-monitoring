using Microsoft.Extensions.Caching.Distributed;

namespace ServerMonitoring.API.Middleware;

/// <summary>
/// Idempotency middleware to prevent duplicate requests
/// Uses Idempotency-Key header for POST/PUT/PATCH operations
/// </summary>
public class IdempotencyMiddleware
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<IdempotencyMiddleware> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

    public IdempotencyMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to POST, PUT, PATCH operations
        if (!IsIdempotentMethod(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Check if Idempotency-Key header is present
        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"idempotency:{idempotencyKey}";

        // Check if request was already processed
        var cachedResponse = await _cache.GetStringAsync(cacheKey);
        if (cachedResponse != null)
        {
            _logger.LogInformation(
                "Idempotent request detected - returning cached response for key: {IdempotencyKey}",
                idempotencyKey);

            var response = System.Text.Json.JsonSerializer.Deserialize<CachedIdempotencyResponse>(cachedResponse);
            
            if (response != null)
            {
                context.Response.StatusCode = response.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(response.Body);
                return;
            }
        }

        // Capture response for caching
        var originalResponseBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Cache successful responses (2xx status codes)
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(responseBody).ReadToEndAsync();

            var responseToCache = new CachedIdempotencyResponse
            {
                StatusCode = context.Response.StatusCode,
                Body = body
            };

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            };

            await _cache.SetStringAsync(
                cacheKey,
                System.Text.Json.JsonSerializer.Serialize(responseToCache),
                cacheOptions);

            _logger.LogInformation(
                "Cached idempotent request response for key: {IdempotencyKey}",
                idempotencyKey);
        }

        // Copy response back to original stream
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalResponseBody);
    }

    private bool IsIdempotentMethod(string method)
    {
        return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
    }

    private class CachedIdempotencyResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; } = string.Empty;
    }
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}

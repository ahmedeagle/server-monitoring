namespace ServerMonitoring.API.Middleware;

/// <summary>
/// Middleware to add correlation ID to every request for distributed tracing
/// Implements best practice for tracking requests across services
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or generate correlation ID
        string correlationId = GetOrGenerateCorrelationId(context);

        // Add to response headers for client tracking
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        // Add to HttpContext items for access in controllers/services
        context.Items[CorrelationIdHeaderName] = correlationId;

        // Add to logging scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdHeaderName] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            _logger.LogInformation(
                "Request started: {Method} {Path} - CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await _next(context);

            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                correlationId);
        }
    }

    private string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Check if client sent correlation ID
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate new GUID
        return Guid.NewGuid().ToString();
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}

using System.Diagnostics;
using System.Text;

namespace ServerMonitoring.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        var stopwatch = Stopwatch.StartNew();

        // Log request
        await LogRequest(context, correlationId);

        // Copy the original response body stream
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Log response
            stopwatch.Stop();
            await LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string correlationId)
    {
        var request = context.Request;

        // Skip logging for health checks and hangfire
        if (request.Path.StartsWithSegments("/health") || 
            request.Path.StartsWithSegments("/hangfire"))
        {
            return;
        }

        var requestBody = await ReadBodyAsync(request);

        _logger.LogInformation(
            "HTTP Request {Method} {Path} | CorrelationId: {CorrelationId} | Body: {Body}",
            request.Method,
            request.Path,
            correlationId,
            requestBody);
    }

    private async Task LogResponse(HttpContext context, string correlationId, long elapsedMs)
    {
        var request = context.Request;

        // Skip logging for health checks and hangfire
        if (request.Path.StartsWithSegments("/health") || 
            request.Path.StartsWithSegments("/hangfire"))
        {
            return;
        }

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation(
            "HTTP Response {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | CorrelationId: {CorrelationId}",
            request.Method,
            request.Path,
            context.Response.StatusCode,
            elapsedMs,
            correlationId);
    }

    private static async Task<string> ReadBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return string.IsNullOrEmpty(body) ? "empty" : body;
    }
}

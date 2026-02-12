using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace ServerMonitoring.Infrastructure.Resilience;

/// <summary>
/// Centralized resilience policies using Polly for fault tolerance
/// Implements retry, circuit breaker, and timeout patterns
/// </summary>
public class ResiliencePolicies
{
    private readonly ILogger<ResiliencePolicies> _logger;

    public ResiliencePolicies(ILogger<ResiliencePolicies> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retry policy with exponential backoff for transient failures
    /// </summary>
    public AsyncRetryPolicy GetRetryPolicy(int maxRetries = 3)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 2s, 4s, 8s
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retry {RetryCount} after {Delay}s due to: {ExceptionMessage}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Circuit breaker policy to prevent cascading failures
    /// Opens circuit after consecutive failures, closes after success
    /// </summary>
    public AsyncCircuitBreakerPolicy GetCircuitBreakerPolicy(
        int exceptionsBeforeBreaking = 5,
        int durationOfBreakInSeconds = 30)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: exceptionsBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(durationOfBreakInSeconds),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError(
                        exception,
                        "Circuit breaker OPENED for {Duration}s due to: {ExceptionMessage}",
                        duration.TotalSeconds,
                        exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker RESET - allowing requests again");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit breaker HALF-OPEN - testing if service recovered");
                });
    }

    /// <summary>
    /// Timeout policy to prevent indefinite waits
    /// </summary>
    public AsyncTimeoutPolicy GetTimeoutPolicy(int timeoutInSeconds = 10)
    {
        return Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(timeoutInSeconds),
                timeoutStrategy: TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timeSpan, task) =>
                {
                    _logger.LogWarning(
                        "Operation timed out after {Timeout}s",
                        timeSpan.TotalSeconds);
                    return Task.CompletedTask;
                });
    }

    /// <summary>
    /// Combined policy: Timeout → Retry → Circuit Breaker
    /// Best practice order: specific to general
    /// </summary>
    public IAsyncPolicy GetCombinedPolicy(
        int maxRetries = 3,
        int exceptionsBeforeBreaking = 5,
        int timeoutInSeconds = 10)
    {
        var timeoutPolicy = GetTimeoutPolicy(timeoutInSeconds);
        var retryPolicy = GetRetryPolicy(maxRetries);
        var circuitBreakerPolicy = GetCircuitBreakerPolicy(exceptionsBeforeBreaking);

        // Wrap policies: innermost executes first
        return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);
    }

    /// <summary>
    /// Database-specific resilience policy for transient errors
    /// </summary>
    public AsyncRetryPolicy GetDatabaseRetryPolicy(int maxRetries = 3)
    {
        return Policy
            .Handle<Exception>(ex => 
                ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase))
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromMilliseconds(100 * retryAttempt), // 100ms, 200ms, 300ms
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Database retry {RetryCount}/{MaxRetries} after {Delay}ms",
                        retryCount,
                        maxRetries,
                        timeSpan.TotalMilliseconds);
                });
    }

    /// <summary>
    /// External API call resilience policy with bulkhead isolation
    /// </summary>
    public IAsyncPolicy GetExternalApiPolicy()
    {
        var bulkheadPolicy = Policy.BulkheadAsync(
            maxParallelization: 10,
            maxQueuingActions: 20,
            onBulkheadRejectedAsync: context =>
            {
                _logger.LogWarning("Bulkhead rejected - too many concurrent requests");
                return Task.CompletedTask;
            });

        return Policy.WrapAsync(
            GetCircuitBreakerPolicy(exceptionsBeforeBreaking: 3),
            GetRetryPolicy(maxRetries: 2),
            bulkheadPolicy,
            GetTimeoutPolicy(timeoutInSeconds: 30));
    }
}

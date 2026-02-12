using MediatR;

namespace ServerMonitoring.Application.Behaviors;

/// <summary>
/// Logging behavior for MediatR pipeline
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Simple pass-through for now
        return await next();
    }
}

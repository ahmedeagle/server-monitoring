using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Interfaces;
using ServerMonitoring.Infrastructure.Data;
using ServerMonitoring.Infrastructure.Repositories;
using ServerMonitoring.Infrastructure.Resilience;
using ServerMonitoring.Infrastructure.Services;

namespace ServerMonitoring.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Unit of Work - coordinates multiple repositories and transactions
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories - Base repository only (ServerRepository already implemented)
        services.AddScoped<IServerRepository, ServerRepository>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMetricsCollector, ResilientMetricsCollector>();
        services.AddScoped<ICacheService, CacheService>();

        // Resilience
        services.AddSingleton<ResiliencePolicies>();

        return services;
    }
}

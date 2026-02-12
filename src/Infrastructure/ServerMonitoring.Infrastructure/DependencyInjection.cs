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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IServerRepository, ServerRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMetricsCollector, ResilientMetricsCollector>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddSingleton<ResiliencePolicies>();

        return services;
    }
}

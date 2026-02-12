using Hangfire.Dashboard;

namespace ServerMonitoring.API.Middleware;

/// <summary>
/// Authorization filter for Hangfire Dashboard
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return true;
    }
}

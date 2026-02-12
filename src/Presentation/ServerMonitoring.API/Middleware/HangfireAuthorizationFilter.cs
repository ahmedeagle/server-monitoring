using Hangfire.Dashboard;

namespace ServerMonitoring.API.Middleware;

/// <summary>
/// Authorization filter for Hangfire Dashboard
/// In production, implement proper authentication/authorization
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow all authenticated users in development
        // In production, restrict to Admin role:
        // return httpContext.User.IsInRole("Admin");
        
        return true; // Allow all in development
    }
}

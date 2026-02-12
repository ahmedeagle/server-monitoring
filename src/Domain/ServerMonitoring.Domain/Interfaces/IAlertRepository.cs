using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Domain.Interfaces;

/// <summary>
/// Repository interface for Alert entity
/// </summary>
public interface IAlertRepository : IRepository<Alert>
{
    /// <summary>
    /// Gets alerts for a specific server
    /// </summary>
    Task<List<Alert>> GetByServerIdAsync(int serverId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets unacknowledged alerts
    /// </summary>
    Task<List<Alert>> GetUnacknowledgedAlertsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets unresolved alerts
    /// </summary>
    Task<List<Alert>> GetUnresolvedAlertsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets alerts by severity
    /// </summary>
    Task<List<Alert>> GetBySeverityAsync(string severity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets alerts created within a date range
    /// </summary>
    Task<List<Alert>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets count of critical unresolved alerts
    /// </summary>
    Task<int> GetCriticalUnresolvedCountAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Acknowledges an alert
    /// </summary>
    Task AcknowledgeAsync(int alertId, int acknowledgedByUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resolves an alert
    /// </summary>
    Task ResolveAsync(int alertId, int resolvedByUserId, CancellationToken cancellationToken = default);
}

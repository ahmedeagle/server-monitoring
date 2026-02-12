using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Domain.Interfaces;

/// <summary>
/// Repository interface for Metric entity (time-series data)
/// </summary>
public interface IMetricRepository : IRepository<Metric>
{
    /// <summary>
    /// Gets metrics for a specific server
    /// </summary>
    Task<List<Metric>> GetByServerIdAsync(int serverId, int limit = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets metrics for a server within a time range
    /// </summary>
    Task<List<Metric>> GetByServerIdAndDateRangeAsync(
        int serverId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets latest metric for a server
    /// </summary>
    Task<Metric?> GetLatestByServerIdAsync(int serverId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets average metrics for a server over a period
    /// </summary>
    Task<(double avgCpu, double avgMemory, double avgDisk, double avgResponse)> GetAverageMetricsAsync(
        int serverId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes old metrics older than specified date (for data retention)
    /// </summary>
    Task DeleteOlderThanAsync(DateTime date, CancellationToken cancellationToken = default);
}

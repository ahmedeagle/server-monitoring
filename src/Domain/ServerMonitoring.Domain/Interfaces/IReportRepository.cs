using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Domain.Interfaces;

/// <summary>
/// Repository interface for Report entity
/// </summary>
public interface IReportRepository : IRepository<Report>
{
    /// <summary>
    /// Gets reports by status
    /// </summary>
    Task<List<Report>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets pending reports
    /// </summary>
    Task<List<Report>> GetPendingReportsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets reports for a specific user
    /// </summary>
    Task<List<Report>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets reports created within a date range
    /// </summary>
    Task<List<Report>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates report status
    /// </summary>
    Task UpdateStatusAsync(int reportId, string status, string? filePath = null, CancellationToken cancellationToken = default);
}

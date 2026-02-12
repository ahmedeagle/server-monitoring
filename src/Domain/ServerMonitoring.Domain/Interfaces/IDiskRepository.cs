using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Domain.Interfaces;

/// <summary>
/// Repository interface for Disk entity
/// </summary>
public interface IDiskRepository : IRepository<Disk>
{
    /// <summary>
    /// Gets disks for a specific server
    /// </summary>
    Task<List<Disk>> GetByServerIdAsync(int serverId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets latest disk information for a server
    /// </summary>
    Task<List<Disk>> GetLatestByServerIdAsync(int serverId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets disks exceeding usage threshold
    /// </summary>
    Task<List<Disk>> GetDisksExceedingThresholdAsync(double thresholdPercentage, CancellationToken cancellationToken = default);
}

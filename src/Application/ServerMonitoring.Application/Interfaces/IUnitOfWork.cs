using Microsoft.EntityFrameworkCore.Storage;
using ServerMonitoring.Domain.Interfaces;

namespace ServerMonitoring.Application.Interfaces;

/// <summary>
/// Unit of Work pattern for managing repositories and transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Server aggregate repository</summary>
    IServerRepository Servers { get; }
    
    /// <summary>User aggregate repository</summary>
    IUserRepository Users { get; }
    
    /// <summary>Role repository</summary>
    IRoleRepository Roles { get; }
    
    /// <summary>Metric repository for time-series data</summary>
    IMetricRepository Metrics { get; }
    
    /// <summary>Disk information repository</summary>
    IDiskRepository Disks { get; }
    
    /// <summary>Alert repository</summary>
    IAlertRepository Alerts { get; }
    
    /// <summary>Report repository</summary>
    IReportRepository Reports { get; }
    
    /// <summary>UserRole many-to-many repository</summary>
    IUserRoleRepository UserRoles { get; }

    /// <summary>Saves all pending changes to the database</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Begins a new database transaction</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Commits the current transaction</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Rolls back the current transaction</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

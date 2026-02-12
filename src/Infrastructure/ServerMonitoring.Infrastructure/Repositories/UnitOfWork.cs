using Microsoft.EntityFrameworkCore.Storage;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Interfaces;
using ServerMonitoring.Infrastructure.Data;

namespace ServerMonitoring.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation coordinating multiple repositories and transactions.
/// Ensures atomic operations across multiple aggregates following DDD principles.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // Repository instances - using generic Repository<T> for non-specialized repositories
    private IServerRepository? _servers;
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IMetricRepository? _metrics;
    private IDiskRepository? _disks;
    private IAlertRepository? _alerts;
    private IReportRepository? _reports;
    private IUserRoleRepository? _userRoles;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Only ServerRepository is specialized, others use generic Repository<T>
    public IServerRepository Servers => _servers ??= new ServerRepository(_context);
    public IUserRepository Users => _users ??= (IUserRepository)new Repository<Domain.Entities.User>(_context);
    public IRoleRepository Roles => _roles ??= (IRoleRepository)new Repository<Domain.Entities.Role>(_context);
    public IMetricRepository Metrics => _metrics ??= (IMetricRepository)new Repository<Domain.Entities.Metric>(_context);
    public IDiskRepository Disks => _disks ??= (IDiskRepository)new Repository<Domain.Entities.Disk>(_context);
    public IAlertRepository Alerts => _alerts ??= (IAlertRepository)new Repository<Domain.Entities.Alert>(_context);
    public IReportRepository Reports => _reports ??= (IReportRepository)new Repository<Domain.Entities.Report>(_context);
    public IUserRoleRepository UserRoles => _userRoles ??= (IUserRoleRepository)new Repository<Domain.Entities.UserRole>(_context);

    /// <summary>
    /// Saves all pending changes in a single transaction
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction for complex operations
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}

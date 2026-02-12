using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Domain.Interfaces;

/// <summary>
/// Repository interface for Role entity
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Gets a role by name
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets default user role
    /// </summary>
    Task<Role?> GetDefaultRoleAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets admin role
    /// </summary>
    Task<Role?> GetAdminRoleAsync(CancellationToken cancellationToken = default);
}

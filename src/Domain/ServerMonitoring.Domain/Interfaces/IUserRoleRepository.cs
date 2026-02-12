using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Domain.Interfaces;

/// <summary>
/// Repository interface for UserRole many-to-many relationship
/// </summary>
public interface IUserRoleRepository : IRepository<UserRole>
{
    /// <summary>
    /// Gets all roles for a specific user
    /// </summary>
    Task<List<Role>> GetRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users with a specific role
    /// </summary>
    Task<List<User>> GetUsersByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a role to a user
    /// </summary>
    Task AddRoleToUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a role from a user
    /// </summary>
    Task RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);
}

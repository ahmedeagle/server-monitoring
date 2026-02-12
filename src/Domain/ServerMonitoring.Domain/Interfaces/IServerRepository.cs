using ServerMonitoring.Domain.Interfaces;

namespace ServerMonitoring.Domain.Interfaces;

public partial interface IServerRepository : IRepository<Entities.Server>
{
    /// <summary>
    /// Get all servers as IQueryable for advanced querying and pagination
    /// </summary>
    IQueryable<Entities.Server> GetAllAsQueryable();
    
    Task<Entities.Server?> GetByNameAsync(string name);
    Task<Entities.Server?> GetByIPAddressAsync(string ipAddress);
    Task<List<Entities.Server>> GetByStatusAsync(Enums.ServerStatus status);
}

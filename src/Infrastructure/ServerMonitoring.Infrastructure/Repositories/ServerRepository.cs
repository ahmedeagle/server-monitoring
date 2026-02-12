using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Enums;
using ServerMonitoring.Domain.Interfaces;
using ServerMonitoring.Infrastructure.Data;

namespace ServerMonitoring.Infrastructure.Repositories;

public class ServerRepository : Repository<Server>, IServerRepository
{
    public ServerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public IQueryable<Server> GetAllAsQueryable()
    {
        // For queries that will be modified (filtered, sorted, paginated)
        // AsNoTracking is applied in the repository layer for read operations
        return _dbSet.AsNoTracking();
    }

    public async Task<Server?> GetByNameAsync(string name)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task<Server?> GetByIPAddressAsync(string ipAddress)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IPAddress == ipAddress);
    }

    public async Task<List<Server>> GetByStatusAsync(ServerStatus status)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.Status == status)
            .ToListAsync();
    }
}

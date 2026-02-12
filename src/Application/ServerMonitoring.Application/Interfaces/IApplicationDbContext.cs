using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Server> Servers { get; }
    DbSet<Metric> Metrics { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Disk> Disks { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<Report> Reports { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

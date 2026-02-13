using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Interfaces;
using ServerMonitoring.Infrastructure.Data;

namespace ServerMonitoring.Infrastructure.Repositories;

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Report>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.Status.ToString() == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Report>> GetPendingReportsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.Status == Domain.Enums.ReportStatus.Processing)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Report>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.GeneratedByUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Report>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(int reportId, string status, string? filePath = null, CancellationToken cancellationToken = default)
    {
        var report = await _dbSet.FindAsync(new object[] { reportId }, cancellationToken);
        if (report != null)
        {
            report.Status = Enum.Parse<Domain.Enums.ReportStatus>(status);
            report.UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(filePath))
            {
                report.FilePath = filePath;
                report.GeneratedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

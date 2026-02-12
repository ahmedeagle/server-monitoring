using ServerMonitoring.Domain.Common;
using ServerMonitoring.Domain.Enums;

namespace ServerMonitoring.Domain.Entities;

public class Metric : IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;
    
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkInbound { get; set; }
    public double NetworkOutbound { get; set; }
    public double ResponseTime { get; set; }
    
    public MetricStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    
    // Auditing
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}

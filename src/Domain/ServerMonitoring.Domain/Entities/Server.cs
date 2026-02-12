using ServerMonitoring.Domain.Common;

namespace ServerMonitoring.Domain.Entities;

/// <summary>
/// Server entity representing a monitored server
/// </summary>
public class Server : IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
    public Enums.ServerStatus Status { get; set; }
    
    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
    public ICollection<Disk> Disks { get; set; } = new List<Disk>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    // ISoftDelete methods
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

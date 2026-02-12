using ServerMonitoring.Domain.Common;

namespace ServerMonitoring.Domain.Entities;

public class Disk : IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public int ServerId { get; set; }
    public string DriveLetter { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public long TotalSizeBytes { get; set; }
    public long FreeSpaceBytes { get; set; }
    public string FileSystem { get; set; } = string.Empty;
    public bool IsReady { get; set; } = true;

    // Calculated property
    public decimal UsagePercentage => TotalSizeBytes > 0 
        ? ((TotalSizeBytes - FreeSpaceBytes) / (decimal)TotalSizeBytes) * 100 
        : 0;

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
    public Server Server { get; set; } = null!;

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

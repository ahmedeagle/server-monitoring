using ServerMonitoring.Domain.Common;
using ServerMonitoring.Domain.Enums;

namespace ServerMonitoring.Domain.Entities;

public class Report : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? FilePath { get; set; }
    public string? FileFormat { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? GeneratedByUserId { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public string? ErrorMessage { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public User? GeneratedByUser { get; set; }
}

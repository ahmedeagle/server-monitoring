using ServerMonitoring.Domain.Common;
using ServerMonitoring.Domain.Enums;

namespace ServerMonitoring.Domain.Entities;

public class Alert : IAuditable
{
    public int Id { get; set; }
    public int ServerId { get; set; }
    public int? UserId { get; set; }
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public decimal ActualValue { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Server Server { get; set; } = null!;
    public User? User { get; set; }
}

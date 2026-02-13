namespace ServerMonitoring.Application.DTOs;

public class AlertDto
{
    public int Id { get; set; }
    public int ServerId { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double? ThresholdValue { get; set; }
    public double? ActualValue { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateServerDto
{
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateServerDto
{
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class MetricDto
{
    public int Id { get; set; }
    public int ServerId { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkInbound { get; set; }
    public double NetworkOutbound { get; set; }
    public double ResponseTime { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class CreateMetricDto
{
    public int ServerId { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkInbound { get; set; }
    public double NetworkOutbound { get; set; }
    public double ResponseTime { get; set; }
}

public class ReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? FilePath { get; set; }
    public string? FileFormat { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

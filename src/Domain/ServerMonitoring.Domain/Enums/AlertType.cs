namespace ServerMonitoring.Domain.Enums;

public enum AlertType
{
    CpuUsage = 1,
    MemoryUsage = 2,
    DiskUsage = 3,
    DiskSpace = 4,
    ServerDown = 5,
    ServiceDown = 6,
    Custom = 99
}

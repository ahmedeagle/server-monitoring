namespace ServerMonitoring.Domain.Enums;

/// <summary>
/// Server status enumeration
/// </summary>
public enum ServerStatus
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Maintenance = 3,
    Error = 4
}

namespace ServerMonitoring.Domain.Enums;

public enum ReportType
{
    DailyMetrics = 1,
    WeeklyMetrics = 2,
    MonthlyMetrics = 3,
    CustomPeriod = 4,
    AlertSummary = 5,
    ServerHealth = 6
}

using Microsoft.Extensions.Logging;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Enums;
using ServerMonitoring.Domain.Interfaces;

namespace ServerMonitoring.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire fire-and-forget job for generating performance reports
/// Generates reports for specified servers and time ranges
/// </summary>
public class ReportGenerationJob
{
    private readonly IServerRepository _serverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportGenerationJob> _logger;

    public ReportGenerationJob(
        IServerRepository serverRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReportGenerationJob> logger)
    {
        _serverRepository = serverRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Generates a performance report for the specified parameters
    /// </summary>
    /// <param name="reportId">Report ID from database</param>
    /// <param name="serverId">Server ID to generate report for</param>
    /// <param name="startDate">Start date for report data</param>
    /// <param name="endDate">End date for report data</param>
    public async Task GenerateReportAsync(int reportId, int serverId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Starting report generation for server {ServerId}, Report ID: {ReportId}", 
            serverId, reportId);

        try
        {
            // Update report status to Processing
            await UpdateReportStatusAsync(reportId, ReportStatus.Processing);

            await Task.Delay(TimeSpan.FromSeconds(5));

            var reportData = await GenerateReportDataAsync(serverId, startDate, endDate);

            // Save report to storage (file system, blob storage, etc.)
            var reportPath = await SaveReportAsync(reportId.ToString(), reportData);

            // Update report status to Completed
            await UpdateReportStatusAsync(reportId, ReportStatus.Completed, reportPath);

            _logger.LogInformation("Report generation completed successfully. Report ID: {ReportId}, Path: {Path}", 
                reportId, reportPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report generation failed for Report ID: {ReportId}", reportId);
            await UpdateReportStatusAsync(reportId, ReportStatus.Failed, errorMessage: ex.Message);
            throw;
        }
    }

    private async Task<object> GenerateReportDataAsync(int serverId, DateTime startDate, DateTime endDate)
    {
        // In production, query metrics from database
        var random = new Random();
        
        return new
        {
            ServerId = serverId,
            ServerName = $"Server-{serverId:D2}",
            Period = new { StartDate = startDate, EndDate = endDate },
            Metrics = new
            {
                AverageCpu = random.Next(40, 70),
                AverageMemory = random.Next(50, 80),
                AverageDisk = random.Next(30, 60),
                MaxCpu = random.Next(80, 95),
                MaxMemory = random.Next(85, 95),
                Uptime = 99.9,
                TotalRequests = random.Next(100000, 500000),
                AverageResponseTime = random.Next(100, 300)
            },
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<string> SaveReportAsync(string reportId, object reportData)
    {
        // In production, save to file system or blob storage
        var reportPath = $"/reports/{reportId}.pdf";
        
        // Simulate saving
        await Task.Delay(100);
        
        return reportPath;
    }

    private async Task UpdateReportStatusAsync(int reportId, ReportStatus status, string? path = null, string? errorMessage = null)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
        {
            _logger.LogWarning("Report {ReportId} not found in database", reportId);
            return;
        }

        report.Status = status;
        report.UpdatedAt = DateTime.UtcNow;

        if (status == ReportStatus.Completed && !string.IsNullOrEmpty(path))
        {
            report.FilePath = path;
            report.FileFormat = "PDF";
            report.FileSizeBytes = 524288; // Simulated file size
            report.GeneratedAt = DateTime.UtcNow;
        }

        if (status == ReportStatus.Failed && !string.IsNullOrEmpty(errorMessage))
        {
            report.ErrorMessage = errorMessage;
        }

        _unitOfWork.Reports.Update(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Report {ReportId} status updated to {Status}", reportId, status);
    }

    /// <summary>
    /// Continuation job: Process and notify after report generation completes
    /// This demonstrates Hangfire continuation jobs
    /// </summary>
    /// <param name="reportId">The completed report ID</param>
    public async Task NotifyReportCompletionAsync(int reportId)
    {
        _logger.LogInformation("Processing continuation job for completed report {ReportId}", reportId);

        try
        {
            // Simulate post-processing tasks
            await Task.Delay(1000);

            // Send notification (email, SignalR, etc.)
            _logger.LogInformation("Notification sent for report {ReportId}", reportId);

            // Update analytics or trigger other workflows
            _logger.LogInformation("Post-processing completed for report {ReportId}", reportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process continuation job for report {ReportId}", reportId);
            throw;
        }
    }
}

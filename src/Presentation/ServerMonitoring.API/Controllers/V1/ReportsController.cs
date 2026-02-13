using Asp.Versioning;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Features.Reports.Queries;
using ServerMonitoring.Infrastructure.BackgroundJobs;

namespace ServerMonitoring.API.Controllers.V1;

public class GenerateReportRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Controller for managing performance reports
/// Demonstrates Hangfire fire-and-forget and delayed jobs
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all recent reports from database
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllReports()
    {
        var result = await _mediator.Send(new GetAllReportsQuery());
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Generate a performance report (Fire-and-Forget Job)
    /// </summary>
    /// <param name="request">Report generation parameters</param>
    /// <returns>Report ID for tracking</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GenerateReport([FromBody] GenerateReportRequest request)
    {
        try
        {
            var reportId = Guid.NewGuid().ToString();
            var serverId = 1; // Default server for demo

            // Enqueue fire-and-forget job
            var jobId = BackgroundJob.Enqueue<ReportGenerationJob>(job => 
                job.GenerateReportAsync(serverId, request.StartDate, request.EndDate, reportId));

            // Continuation job: Process after report generation completes
            BackgroundJob.ContinueJobWith<ReportGenerationJob>(jobId, job => 
                job.NotifyReportCompletionAsync(reportId));

            _logger.LogInformation("Report generation enqueued. Title: {Title}, Type: {Type}, Report: {ReportId}, JobId: {JobId}", 
                request.Title, request.Type, reportId, jobId);

            return Accepted(new
            {
                ReportId = reportId,
                ServerId = serverId,
                Period = new { StartDate = request.StartDate, EndDate = request.EndDate },
                Status = "Queued",
                Message = "Report generation has been queued. Check status with the report ID."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue report generation");
            return BadRequest(new { Error = "Failed to queue report generation" });
        }
    }

    /// <summary>
    /// Schedule a report for future generation (Delayed Job)
    /// </summary>
    /// <param name="serverId">Server ID to generate report for</param>
    /// <param name="delayMinutes">Minutes to delay before generating</param>
    /// <returns>Scheduled report ID</returns>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    public IActionResult ScheduleReport([FromQuery] int serverId, [FromQuery] int delayMinutes = 60)
    {
        var reportId = Guid.NewGuid().ToString();
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Schedule delayed job
        BackgroundJob.Schedule<ReportGenerationJob>(
            job => job.GenerateReportAsync(serverId, startDate, endDate, reportId),
            TimeSpan.FromMinutes(delayMinutes));

        _logger.LogInformation("Report generation scheduled in {Delay} minutes. Server: {ServerId}, Report: {ReportId}", 
            delayMinutes, serverId, reportId);

        return Accepted(new
        {
            ReportId = reportId,
            ServerId = serverId,
            ScheduledFor = DateTime.UtcNow.AddMinutes(delayMinutes),
            Status = "Scheduled"
        });
    }

    /// <summary>
    /// Trigger immediate metrics collection (Fire-and-Forget)
    /// </summary>
    [HttpPost("collect-metrics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult TriggerMetricsCollection()
    {
        var jobId = BackgroundJob.Enqueue<MetricsCollectionJob>(job => job.CollectMetricsAsync());

        _logger.LogInformation("Metrics collection triggered manually. Job ID: {JobId}", jobId);

        return Accepted(new
        {
            JobId = jobId,
            Message = "Metrics collection has been triggered",
            Status = "Queued"
        });
    }

    /// <summary>
    /// Get report status (Mock implementation)
    /// </summary>
    /// <param name="reportId">Report ID to check</param>
    [HttpGet("{reportId}/status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetReportStatus(string reportId)
    {
        // In production, query report status from database
        return Ok(new
        {
            ReportId = reportId,
            Status = "Processing",
            Progress = 65,
            EstimatedCompletion = DateTime.UtcNow.AddMinutes(2)
        });
    }

    /// <summary>
    /// Download a generated report
    /// </summary>
    /// <param name="reportId">Report ID to download</param>
    [HttpGet("{reportId}/download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadReport(int reportId)
    {
        try
        {
            // Query report from database using MediatR
            var result = await _mediator.Send(new GetAllReportsQuery());
            
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound(new { Error = "Report not found" });
            }

            var report = result.Data.FirstOrDefault(r => r.Id == reportId);
            
            if (report == null)
            {
                return NotFound(new { Error = $"Report with ID {reportId} not found" });
            }

            // Generate CSV content from report
            var csvContent = GenerateReportCsv(report);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

            _logger.LogInformation("Report {ReportId} downloaded", reportId);

            return File(bytes, "text/csv", $"report_{reportId}_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading report {ReportId}", reportId);
            return StatusCode(500, new { Error = "Failed to download report" });
        }
    }

    private string GenerateReportCsv(ReportDto report)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Report Details");
        csv.AppendLine($"Report ID,{report.Id}");
        csv.AppendLine($"Title,\"{report.Title}\"");
        csv.AppendLine($"Description,\"{report.Description}\"");
        csv.AppendLine($"Type,{report.Type}");
        csv.AppendLine($"Status,{report.Status}");
        csv.AppendLine($"Start Date,{report.StartDate:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"End Date,{report.EndDate:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Created At,{report.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        
        if (report.GeneratedAt.HasValue)
        {
            csv.AppendLine($"Generated At,{report.GeneratedAt.Value:yyyy-MM-dd HH:mm:ss}");
        }
        
        if (!string.IsNullOrEmpty(report.FilePath))
        {
            csv.AppendLine($"File Path,{report.FilePath}");
            csv.AppendLine($"File Format,{report.FileFormat}");
            csv.AppendLine($"File Size (bytes),{report.FileSizeBytes}");
        }
        
        return csv.ToString();
    }
}

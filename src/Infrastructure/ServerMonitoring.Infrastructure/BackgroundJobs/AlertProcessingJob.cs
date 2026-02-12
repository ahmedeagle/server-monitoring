using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Enums;

namespace ServerMonitoring.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire background job for processing alerts
/// Checks metric thresholds and generates alerts when exceeded
/// </summary>
public class AlertProcessingJob
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AlertProcessingJob> _logger;

    public AlertProcessingJob(
        IApplicationDbContext context,
        ILogger<AlertProcessingJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Processes alert rules and generates alerts for threshold breaches
    /// </summary>
    public async Task ProcessAlertsAsync()
    {
        _logger.LogInformation("Starting alert processing job at {Time}", DateTime.UtcNow);

        try
        {
            var servers = await _context.Servers
                .Include(s => s.Metrics.OrderByDescending(m => m.Timestamp).Take(1))
                .Include(s => s.Alerts)
                .ToListAsync();

            int alertsCreated = 0;

            foreach (var server in servers)
            {
                var latestMetric = server.Metrics.FirstOrDefault();
                if (latestMetric == null) continue;

                // Check CPU threshold (80%)
                if (latestMetric.CpuUsage > 80)
                {
                    var existingAlert = server.Alerts
                        .FirstOrDefault(a => a.Type == AlertType.CpuUsage && !a.IsResolved);
                    
                    if (existingAlert == null)
                    {
                        var alert = new Alert
                        {
                            ServerId = server.Id,
                            Type = AlertType.CpuUsage,
                            Severity = latestMetric.CpuUsage > 90 ? AlertSeverity.Critical : AlertSeverity.Warning,
                            Title = $"High CPU Usage on {server.Name}",
                            Message = $"CPU usage is at {latestMetric.CpuUsage:F2}%, exceeding threshold of 80%",
                            ThresholdValue = (decimal)80,
                            ActualValue = (decimal)latestMetric.CpuUsage
                        };
                        _context.Alerts.Add(alert);
                        alertsCreated++;
                        
                        _logger.LogWarning("Alert created: CPU usage on {Server} is at {CpuUsage}%, exceeding threshold of 80%", 
                            server.Name, latestMetric.CpuUsage);
                    }
                }

                // Check Memory threshold (85%)
                if (latestMetric.MemoryUsage > 85)
                {
                    var existingAlert = server.Alerts
                        .FirstOrDefault(a => a.Type == AlertType.MemoryUsage && !a.IsResolved);
                    
                    if (existingAlert == null)
                    {
                        var alert = new Alert
                        {
                            ServerId = server.Id,
                            Type = AlertType.MemoryUsage,
                            Severity = latestMetric.MemoryUsage > 95 ? AlertSeverity.Critical : AlertSeverity.Warning,
                            Title = $"High Memory Usage on {server.Name}",
                            Message = $"Memory usage is at {latestMetric.MemoryUsage:F2}%, exceeding threshold of 85%",
                            ThresholdValue = (decimal)85,
                            ActualValue = (decimal)latestMetric.MemoryUsage
                        };
                        _context.Alerts.Add(alert);
                        alertsCreated++;
                        
                        _logger.LogWarning("Alert created: Memory usage on {Server} is at {MemoryUsage}%, exceeding threshold of 85%",
                            server.Name, latestMetric.MemoryUsage);
                    }
                }

                // Check Disk threshold (90%)
                if (latestMetric.DiskUsage > 90)
                {
                    var existingAlert = server.Alerts
                        .FirstOrDefault(a => a.Type == AlertType.DiskUsage && !a.IsResolved);
                    
                    if (existingAlert == null)
                    {
                        var alert = new Alert
                        {
                            ServerId = server.Id,
                            Type = AlertType.DiskUsage,
                            Severity = latestMetric.DiskUsage > 95 ? AlertSeverity.Critical : AlertSeverity.Error,
                            Title = $"High Disk Usage on {server.Name}",
                            Message = $"Disk usage is at {latestMetric.DiskUsage:F2}%, exceeding threshold of 90%",
                            ThresholdValue = (decimal)90,
                            ActualValue = (decimal)latestMetric.DiskUsage
                        };
                        _context.Alerts.Add(alert);
                        alertsCreated++;
                        
                        _logger.LogWarning("Alert created: Disk usage on {Server} is at {DiskUsage}%, exceeding threshold of 90%",
                            server.Name, latestMetric.DiskUsage);
                    }
                }
            }

            if (alertsCreated > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("{Count} new alerts created and saved", alertsCreated);
            }

            _logger.LogInformation("Alert processing job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Alert processing job failed");
            throw;
        }
    }
}

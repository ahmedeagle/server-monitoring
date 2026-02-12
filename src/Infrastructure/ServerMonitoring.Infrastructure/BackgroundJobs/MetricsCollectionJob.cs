using Microsoft.Extensions.Logging;
using ServerMonitoring.Domain.Interfaces;

namespace ServerMonitoring.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire background job for collecting server metrics
/// Runs every 1-5 minutes to collect CPU, Memory, Disk, Network metrics
/// </summary>
public class MetricsCollectionJob
{
    private readonly IServerRepository _serverRepository;
    private readonly ILogger<MetricsCollectionJob> _logger;

    public MetricsCollectionJob(
        IServerRepository serverRepository,
        ILogger<MetricsCollectionJob> logger)
    {
        _serverRepository = serverRepository;
        _logger = logger;
    }

    /// <summary>
    /// Collects metrics for all active servers
    /// </summary>
    public async Task CollectMetricsAsync()
    {
        _logger.LogInformation("Starting metrics collection job at {Time}", DateTime.UtcNow);

        try
        {
            // Get all active servers
            var servers = await _serverRepository.GetAllAsync();
            var activeServers = servers.Where(s => !s.IsDeleted).ToList();

            _logger.LogInformation("Collecting metrics for {Count} servers", activeServers.Count);

            foreach (var server in activeServers)
            {
                try
                {
                    await CollectServerMetricsAsync(server.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to collect metrics for server {ServerId}", server.Id);
                }
            }

            _logger.LogInformation("Metrics collection job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metrics collection job failed");
            throw;
        }
    }

    /// <summary>
    /// Collects metrics for a specific server
    /// </summary>
    private Task CollectServerMetricsAsync(int serverId)
    {
        var random = new Random();
        
        // Simulate metric collection (in production, this would call actual monitoring APIs)
        var metrics = new
        {
            ServerId = serverId,
            CpuUsage = random.Next(10, 95),
            MemoryUsage = random.Next(30, 90),
            DiskUsage = random.Next(20, 85),
            NetworkUsage = random.Next(5, 60),
            ResponseTime = random.Next(50, 500),
            Timestamp = DateTime.UtcNow
        };

        // In production, save to database via repository
        // await _serverRepository.AddMetricAsync(serverId, metrics);

        // TODO: Send real-time update via SignalR hub
        // await _hubContext.Clients.Group($"server_{serverId}").SendAsync("ReceiveMetrics", metrics);

        _logger.LogDebug("Collected metrics for server {ServerId}: CPU={CPU}%, Memory={Memory}%, Disk={Disk}%",
            serverId, metrics.CpuUsage, metrics.MemoryUsage, metrics.DiskUsage);
        
        return Task.CompletedTask;
    }
}

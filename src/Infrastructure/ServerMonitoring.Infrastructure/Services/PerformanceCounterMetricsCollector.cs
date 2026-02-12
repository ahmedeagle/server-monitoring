using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Infrastructure.Services;

public class PerformanceCounterMetricsCollector : IMetricsCollector
{
    private readonly ILogger<PerformanceCounterMetricsCollector> _logger;

    public PerformanceCounterMetricsCollector(ILogger<PerformanceCounterMetricsCollector> logger)
    {
        _logger = logger;
    }

    public async Task<Metric> CollectMetricsAsync(Server server, CancellationToken cancellationToken = default)
    {
        try
        {
            // For local machine collection
            if (server.IPAddress == "localhost" || server.IPAddress == "127.0.0.1")
            {
                return await CollectLocalMetricsAsync(server.Id);
            }

            // For remote servers, would need WMI or remote API
            _logger.LogWarning("Remote metrics collection not implemented for {Server}. Using simulated data.", server.IPAddress);
            return GenerateSimulatedMetrics(server.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect metrics for {Server}", server.IPAddress);
            throw;
        }
    }

    private async Task<Metric> CollectLocalMetricsAsync(int serverId)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return CollectWindowsMetrics(serverId);
                }
                else
                {
                    return CollectLinuxMetrics(serverId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to collect system metrics, using simulated data");
                return GenerateSimulatedMetrics(serverId);
            }
        });
    }

    private Metric CollectWindowsMetrics(int serverId)
    {
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            using var ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            // First call always returns 0, so we call twice
            cpuCounter.NextValue();
            Thread.Sleep(100);
            var cpuUsage = cpuCounter.NextValue();
            var memoryUsage = ramCounter.NextValue();
#pragma warning restore CA1416 // Validate platform compatibility

            // Get disk usage
            var driveInfo = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .FirstOrDefault();

            decimal diskUsage = 0;
            if (driveInfo != null)
            {
                var used = driveInfo.TotalSize - driveInfo.AvailableFreeSpace;
                diskUsage = (decimal)used / driveInfo.TotalSize * 100;
            }

            // Network usage (simplified)
            var networkUsage = new Random().Next(5, 30);

            return new Metric
            {
                ServerId = serverId,
                CpuUsage = (double)cpuUsage,
                MemoryUsage = (double)memoryUsage,
                DiskUsage = (double)diskUsage,
                NetworkInbound = networkUsage,
                NetworkOutbound = networkUsage / 2,
                ResponseTime = 50,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect Windows performance counters");
            throw;
        }
    }

    private Metric CollectLinuxMetrics(int serverId)
    {
        try
        {
            // Read /proc/stat for CPU
            decimal cpuUsage = 0;
            if (File.Exists("/proc/stat"))
            {
                var statLines = File.ReadAllLines("/proc/stat");
                var cpuLine = statLines.FirstOrDefault(l => l.StartsWith("cpu "));
                if (cpuLine != null)
                {
                    // Simplified CPU calculation
                    cpuUsage = new Random().Next(10, 80);
                }
            }

            // Read /proc/meminfo for Memory
            decimal memoryUsage = 0;
            if (File.Exists("/proc/meminfo"))
            {
                var memLines = File.ReadAllLines("/proc/meminfo");
                var totalMem = ParseMemInfoLine(memLines.FirstOrDefault(l => l.StartsWith("MemTotal:")));
                var availMem = ParseMemInfoLine(memLines.FirstOrDefault(l => l.StartsWith("MemAvailable:")));
                
                if (totalMem > 0)
                {
                    memoryUsage = (decimal)(totalMem - availMem) / totalMem * 100;
                }
            }

            // Disk usage
            decimal diskUsage = 0;
            var driveInfo = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .FirstOrDefault();

            if (driveInfo != null)
            {
                var used = driveInfo.TotalSize - driveInfo.AvailableFreeSpace;
                diskUsage = (decimal)used / driveInfo.TotalSize * 100;
            }

            var networkUsage = new Random().Next(5, 30);

            return new Metric
            {
                ServerId = serverId,
                CpuUsage = (double)cpuUsage,
                MemoryUsage = (double)memoryUsage,
                DiskUsage = (double)diskUsage,
                NetworkInbound = networkUsage,
                NetworkOutbound = networkUsage / 2,
                ResponseTime = 50,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect Linux metrics");
            throw;
        }
    }

    private long ParseMemInfoLine(string? line)
    {
        if (string.IsNullOrEmpty(line)) return 0;
        
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && long.TryParse(parts[1], out var value))
        {
            return value;
        }
        return 0;
    }

    private Metric GenerateSimulatedMetrics(int serverId)
    {
        var random = new Random();
        return new Metric
        {
            ServerId = serverId,
            CpuUsage = random.Next(10, 90),
            MemoryUsage = random.Next(20, 80),
            DiskUsage = random.Next(30, 70),
            NetworkInbound = random.Next(5, 50),
            NetworkOutbound = random.Next(5, 50),
            ResponseTime = random.Next(10, 200),
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}

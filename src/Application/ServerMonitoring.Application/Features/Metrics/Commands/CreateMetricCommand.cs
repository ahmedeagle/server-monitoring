using MediatR;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Common;

namespace ServerMonitoring.Application.Features.Metrics.Commands;

public class CreateMetricCommand : IRequest<Result<MetricDto>>
{
    public int ServerId { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkInbound { get; set; }
    public double NetworkOutbound { get; set; }
    public double ResponseTime { get; set; }
}

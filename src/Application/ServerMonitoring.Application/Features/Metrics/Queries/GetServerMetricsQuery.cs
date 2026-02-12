using MediatR;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.Application.Features.Metrics.Queries;

public class GetServerMetricsQuery : IRequest<Result<List<MetricDto>>>
{
    public int ServerId { get; set; }
    public int Limit { get; set; } = 100;
}

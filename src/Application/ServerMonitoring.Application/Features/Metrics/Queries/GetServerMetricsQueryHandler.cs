using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Metrics.Queries;

public class GetServerMetricsQueryHandler : IRequestHandler<GetServerMetricsQuery, Result<List<MetricDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetServerMetricsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MetricDto>>> Handle(GetServerMetricsQuery request, CancellationToken cancellationToken)
    {
        var metrics = await _context.Metrics
            .Where(m => m.ServerId == request.ServerId)
            .OrderByDescending(m => m.Timestamp)
            .Take(request.Limit)
            .Select(m => new MetricDto
            {
                Id = m.Id,
                ServerId = m.ServerId,
                CpuUsage = m.CpuUsage,
                MemoryUsage = m.MemoryUsage,
                DiskUsage = m.DiskUsage,
                NetworkInbound = m.NetworkInbound,
                NetworkOutbound = m.NetworkOutbound,
                ResponseTime = m.ResponseTime,
                RecordedAt = m.Timestamp
            })
            .ToListAsync(cancellationToken);

        return Result<List<MetricDto>>.Success(metrics);
    }
}

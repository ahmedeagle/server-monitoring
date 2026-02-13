using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Application.Features.Metrics.Commands;

public class CreateMetricCommandHandler : IRequestHandler<CreateMetricCommand, Result<MetricDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateMetricCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MetricDto>> Handle(CreateMetricCommand request, CancellationToken cancellationToken)
    {
        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == request.ServerId && !s.IsDeleted, cancellationToken);

        if (server == null)
        {
            return Result<MetricDto>.Failure("Server not found");
        }

        var metric = new Metric
        {
            ServerId = request.ServerId,
            CpuUsage = request.CpuUsage,
            MemoryUsage = request.MemoryUsage,
            DiskUsage = request.DiskUsage,
            NetworkInbound = request.NetworkInbound,
            NetworkOutbound = request.NetworkOutbound,
            ResponseTime = request.ResponseTime,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Metrics.Add(metric);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MetricDto
        {
            Id = metric.Id,
            ServerId = metric.ServerId,
            CpuUsage = metric.CpuUsage,
            MemoryUsage = metric.MemoryUsage,
            DiskUsage = metric.DiskUsage,
            NetworkInbound = metric.NetworkInbound,
            NetworkOutbound = metric.NetworkOutbound,
            ResponseTime = metric.ResponseTime,
            RecordedAt = metric.Timestamp
        };

        return Result<MetricDto>.Success(dto);
    }
}

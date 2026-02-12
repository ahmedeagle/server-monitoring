using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Alerts.Queries;

public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, Result<List<AlertDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAlertsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AlertDto>>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Alerts.Include(a => a.Server).AsQueryable();

        if (request.UnacknowledgedOnly)
        {
            query = query.Where(a => !a.IsAcknowledged);
        }

        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AlertDto
            {
                Id = a.Id,
                ServerId = a.ServerId,
                ServerName = a.Server.Name,
                Type = a.Type.ToString(),
                Severity = a.Severity.ToString(),
                Title = a.Title,
                Message = a.Message,
                ThresholdValue = (double?)a.ThresholdValue,
                ActualValue = (double?)a.ActualValue,
                IsAcknowledged = a.IsAcknowledged,
                AcknowledgedAt = a.AcknowledgedAt,
                AcknowledgedBy = a.AcknowledgedBy,
                IsResolved = a.IsResolved,
                ResolvedAt = a.ResolvedAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<AlertDto>>.Success(alerts);
    }
}

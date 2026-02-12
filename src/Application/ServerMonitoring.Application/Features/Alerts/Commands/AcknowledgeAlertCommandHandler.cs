using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Alerts.Commands;

public class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public AcknowledgeAlertCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (alert == null)
        {
            return Result<bool>.Failure("Alert not found");
        }

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedBy = "System"; // TODO: Get from auth context

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

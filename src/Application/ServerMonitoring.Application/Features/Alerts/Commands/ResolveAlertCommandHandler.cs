using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Alerts.Commands;

public class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public ResolveAlertCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (alert == null)
        {
            return Result<bool>.Failure("Alert not found");
        }

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
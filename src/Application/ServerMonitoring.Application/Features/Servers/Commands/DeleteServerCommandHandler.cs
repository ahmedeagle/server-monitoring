using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Servers.Commands;

public class DeleteServerCommandHandler : IRequestHandler<DeleteServerCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteServerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteServerCommand request, CancellationToken cancellationToken)
    {
        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted, cancellationToken);

        if (server == null)
        {
            return Result<bool>.Failure("Server not found");
        }

        // Soft delete
        server.IsDeleted = true;
        server.DeletedAt = DateTime.UtcNow;
        server.DeletedBy = "System";

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

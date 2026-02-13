using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Servers.Commands;

public class UpdateServerCommandHandler : IRequestHandler<UpdateServerCommand, Result<ServerDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateServerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServerDto>> Handle(UpdateServerCommand request, CancellationToken cancellationToken)
    {
        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted, cancellationToken);

        if (server == null)
        {
            return Result<ServerDto>.Failure("Server not found");
        }

        server.Name = request.Name;
        server.Hostname = request.Hostname;
        server.IPAddress = request.IPAddress;
        server.Port = request.Port;
        server.OperatingSystem = request.OperatingSystem;
        server.UpdatedAt = DateTime.UtcNow;
        server.UpdatedBy = "System";

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            Hostname = server.Hostname,
            IPAddress = server.IPAddress,
            Port = server.Port,
            OperatingSystem = server.OperatingSystem,
            Status = server.Status.ToString(),
            CreatedAt = server.CreatedAt,
            UpdatedAt = server.UpdatedAt
        };

        return Result<ServerDto>.Success(dto);
    }
}

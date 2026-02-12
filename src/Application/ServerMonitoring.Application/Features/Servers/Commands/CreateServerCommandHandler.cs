using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Application.Features.Servers.Commands;

public class CreateServerCommandHandler : IRequestHandler<CreateServerCommand, Result<ServerDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateServerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServerDto>> Handle(CreateServerCommand request, CancellationToken cancellationToken)
    {
        var server = new Server
        {
            Name = request.Name,
            Hostname = request.Hostname,
            IPAddress = request.IPAddress,
            Port = request.Port,
            OperatingSystem = request.OperatingSystem,
            Status = "Offline",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Get from auth context
        };

        _context.Servers.Add(server);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            Hostname = server.Hostname,
            IPAddress = server.IPAddress,
            Port = server.Port,
            OperatingSystem = server.OperatingSystem,
            Status = server.Status,
            IsActive = server.IsActive,
            CreatedAt = server.CreatedAt,
            UpdatedAt = server.UpdatedAt
        };

        return Result<ServerDto>.Success(dto);
    }
}

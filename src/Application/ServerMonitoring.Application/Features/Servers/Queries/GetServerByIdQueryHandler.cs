using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Servers.Queries;

public class GetServerByIdQueryHandler : IRequestHandler<GetServerByIdQuery, Result<ServerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServerDto>> Handle(GetServerByIdQuery request, CancellationToken cancellationToken)
    {
        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.IsActive, cancellationToken);

        if (server == null)
        {
            return Result<ServerDto>.Failure("Server not found");
        }

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

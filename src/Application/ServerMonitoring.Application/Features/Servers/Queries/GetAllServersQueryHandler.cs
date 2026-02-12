using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features\Servers.Queries;

public class GetAllServersQueryHandler : IRequestHandler<GetAllServersQuery, Result<List<ServerDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllServersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ServerDto>>> Handle(GetAllServersQuery request, CancellationToken cancellationToken)
    {
        var servers = await _context.Servers
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ServerDto
            {
                Id = s.Id,
                Name = s.Name,
                Hostname = s.Hostname,
                IPAddress = s.IPAddress,
                Port = s.Port,
                OperatingSystem = s.OperatingSystem,
                Status = s.Status,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<ServerDto>>.Success(servers);
    }
}

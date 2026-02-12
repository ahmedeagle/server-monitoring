using MediatR;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.Application.Features.Servers.Queries;

/// <summary>
/// Query to get servers with cursor-based pagination (V2 feature)
/// </summary>
public class GetServersCursorQuery : IRequest<Result<CursorPagedResult<ServerDto>>>
{
    public CursorPaginationParams PaginationParams { get; set; } = new();
}

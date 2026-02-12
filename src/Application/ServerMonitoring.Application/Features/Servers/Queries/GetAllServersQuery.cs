using MediatR;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.Application.Features.Servers.Queries;

public class GetAllServersQuery : IRequest<Result<List<ServerDto>>>
{
}

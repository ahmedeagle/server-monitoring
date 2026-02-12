using MediatR;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Common;

namespace ServerMonitoring.Application.Features.Servers.Queries;

public class GetServerByIdQuery : IRequest<Result<ServerDto>>
{
    public int Id { get; set; }
}

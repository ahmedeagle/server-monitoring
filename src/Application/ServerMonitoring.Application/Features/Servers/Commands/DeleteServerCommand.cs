using MediatR;
using ServerMonitoring.Application.Common;

namespace ServerMonitoring.Application.Features.Servers.Commands;

public class DeleteServerCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}

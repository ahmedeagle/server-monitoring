using MediatR;
using ServerMonitoring.Application.Common;

namespace ServerMonitoring.Application.Features.Alerts.Commands;

public class ResolveAlertCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}

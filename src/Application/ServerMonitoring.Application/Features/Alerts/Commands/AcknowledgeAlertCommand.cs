using MediatR;
using ServerMonitoring.Application.Common;

namespace ServerMonitoring.Application.Features.Alerts.Commands;

public class AcknowledgeAlertCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}

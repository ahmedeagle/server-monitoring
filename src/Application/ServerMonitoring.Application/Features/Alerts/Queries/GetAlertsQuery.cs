using MediatR;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.Application.Features.Alerts.Queries;

public class GetAlertsQuery : IRequest<Result<List<AlertDto>>>
{
    public bool UnacknowledgedOnly { get; set; }
}

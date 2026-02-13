using MediatR;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.Application.Features.Reports.Queries;

public class GetAllReportsQuery : IRequest<Result<List<ReportDto>>>
{
}

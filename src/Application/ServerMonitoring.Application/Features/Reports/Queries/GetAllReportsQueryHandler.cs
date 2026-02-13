using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Reports.Queries;

public class GetAllReportsQueryHandler : IRequestHandler<GetAllReportsQuery, Result<List<ReportDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ReportDto>>> Handle(GetAllReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _context.Reports
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .Select(r => new ReportDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Type = r.Type.ToString(),
                Status = r.Status.ToString(),
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                FilePath = r.FilePath,
                FileFormat = r.FileFormat,
                FileSizeBytes = r.FileSizeBytes,
                GeneratedAt = r.GeneratedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<ReportDto>>.Success(reports);
    }
}

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;

namespace ServerMonitoring.Application.Features.Servers.Queries;

/// <summary>
/// Handler for cursor-based server pagination query
/// </summary>
public class GetServersCursorQueryHandler : IRequestHandler<GetServersCursorQuery, Result<CursorPagedResult<ServerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetServersCursorQueryHandler> _logger;

    public GetServersCursorQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetServersCursorQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CursorPagedResult<ServerDto>>> Handle(
        GetServersCursorQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var serversQuery = _unitOfWork.Servers.GetAllAsQueryable();

            var pagedResult = await serversQuery.ToCursorPagedListAsync(
                server => server.Id,
                request.PaginationParams,
                cancellationToken);

            var serverDtos = _mapper.Map<List<ServerDto>>(pagedResult.Items);

            var result = new CursorPagedResult<ServerDto>
            {
                Items = serverDtos,
                NextCursor = pagedResult.NextCursor,
                PreviousCursor = pagedResult.PreviousCursor,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount
            };

            _logger.LogInformation(
                "Retrieved {Count} servers using cursor pagination. HasNext: {HasNext}",
                serverDtos.Count,
                result.HasNextPage);

            return Result<CursorPagedResult<ServerDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving servers with cursor pagination");
            return Result<CursorPagedResult<ServerDto>>.Failure("Failed to retrieve servers");
        }
    }
}

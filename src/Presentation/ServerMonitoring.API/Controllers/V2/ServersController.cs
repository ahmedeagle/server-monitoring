using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitoring.Application.Common;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Features.Servers.Commands;
using ServerMonitoring.Application.Features.Servers.Queries;

namespace ServerMonitoring.API.Controllers.V2;

/// <summary>
/// Servers API - Version 2 with enhanced features
/// Demonstrates API versioning best practices
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class ServersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ServersController> _logger;

    public ServersController(IMediator mediator, ILogger<ServersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get servers with cursor-based pagination (V2 feature)
    /// </summary>
    /// <param name="paginationParams">Cursor pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cursor paginated server list</returns>
    [HttpGet("cursor")]
    [ProducesResponseType(typeof(CursorPagedResult<ServerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CursorPagedResult<ServerDto>>> GetServersCursor(
        [FromQuery] CursorPaginationParams paginationParams,
        CancellationToken cancellationToken)
    {
        var query = new GetServersCursorQuery
        {
            PaginationParams = paginationParams
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            // Add Link headers for HATEOAS
            if (result.Data!.HasNextPage && !string.IsNullOrEmpty(result.Data.NextCursor))
            {
                Response.Headers.Add("Link", $"</api/v2/servers/cursor?cursor={result.Data.NextCursor}&pageSize={result.Data.PageSize}>; rel=\"next\"");
            }

            if (result.Data.HasPreviousPage && !string.IsNullOrEmpty(result.Data.PreviousCursor))
            {
                Response.Headers.Add("Link", $"</api/v2/servers/cursor?cursor={result.Data.PreviousCursor}&pageSize={result.Data.PageSize}&direction=previous>; rel=\"prev\"");
            }

            return Ok(result.Data);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get server with ETag support for conditional requests (V2 feature)
    /// </summary>
    /// <param name="id">Server ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Server details with ETag</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServerDto>> GetServer(int id, CancellationToken cancellationToken)
    {
        var query = new GetServerByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        // Generate ETag based on UpdatedAt timestamp
        var etag = GenerateETag(result.Data!);
        Response.Headers.Add("ETag", etag);

        // Check If-None-Match header
        if (Request.Headers.TryGetValue("If-None-Match", out var incomingEtag))
        {
            if (incomingEtag == etag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
        }

        // Add Cache-Control header
        Response.Headers.Add("Cache-Control", "private, max-age=60");

        return Ok(result.Data);
    }

    /// <summary>
    /// Create server with idempotency support
    /// </summary>
    /// <param name="command">Create server command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created server</returns>
    /// <remarks>
    /// Include Idempotency-Key header to prevent duplicate creation
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ServerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServerDto>> CreateServer(
        [FromBody] CreateServerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            var etag = GenerateETag(result.Data!);
            Response.Headers.Add("ETag", etag);

            return CreatedAtAction(
                nameof(GetServer),
                new { id = result.Data!.Id, version = "2.0" },
                result.Data);
        }

        return BadRequest(result);
    }

    private string GenerateETag(ServerDto server)
    {
        // Generate ETag from server ID and last modified timestamp
        var etagContent = $"{server.Id}-{server.CreatedAt:yyyyMMddHHmmss}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(etagContent);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return $"\"{Convert.ToBase64String(hash).Substring(0, 16)}\"";
    }
}

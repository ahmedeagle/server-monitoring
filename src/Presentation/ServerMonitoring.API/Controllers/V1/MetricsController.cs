using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Features.Metrics.Commands;
using ServerMonitoring.Application.Features.Metrics.Queries;

namespace ServerMonitoring.API.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IMediator mediator, ILogger<MetricsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<List<MetricDto>>> GetServerMetrics(int serverId, [FromQuery] int limit = 100)
    {
        _logger.LogInformation("Getting metrics for server {ServerId}", serverId);
        var query = new GetServerMetricsQuery { ServerId = serverId, Limit = limit };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MetricDto>> Create([FromBody] CreateMetricDto dto)
    {
        _logger.LogInformation("Creating new metric for server {ServerId}", dto.ServerId);
        
        var command = new CreateMetricCommand
        {
            ServerId = dto.ServerId,
            CpuUsage = dto.CpuUsage,
            MemoryUsage = dto.MemoryUsage,
            DiskUsage = dto.DiskUsage,
            NetworkInbound = dto.NetworkInbound,
            NetworkOutbound = dto.NetworkOutbound,
            ResponseTime = dto.ResponseTime
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        
        return CreatedAtAction(nameof(GetServerMetrics), new { serverId = dto.ServerId }, result.Data);
    }
}

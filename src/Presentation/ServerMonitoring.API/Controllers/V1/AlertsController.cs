using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Features.Alerts.Queries;
using ServerMonitoring.Application.Features.Alerts.Commands;

namespace ServerMonitoring.API.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(IMediator mediator, ILogger<AlertsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> GetAll([FromQuery] bool unacknowledgedOnly = false)
    {
        _logger.LogInformation("Getting alerts (unacknowledged only: {UnacknowledgedOnly})", unacknowledgedOnly);
        var query = new GetAlertsQuery { UnacknowledgedOnly = unacknowledgedOnly };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        
        return Ok(result.Data);
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<List<AlertDto>>> GetServerAlerts(int serverId)
    {
        _logger.LogInformation("Getting alerts for server {ServerId}", serverId);
        var query = new GetAlertsQuery { UnacknowledgedOnly = false };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        
        // Filter by serverId
        var serverAlerts = result.Data?.Where(a => a.ServerId == serverId).ToList() ?? new List<AlertDto>();
        return Ok(serverAlerts);
    }

    [HttpPost("{id}/acknowledge")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        _logger.LogInformation("Acknowledging alert {AlertId}", id);
        var command = new AcknowledgeAlertCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        
        return NoContent();
    }

    [HttpPost("{id}/resolve")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Resolve(int id)
    {
        _logger.LogInformation("Resolving alert {AlertId}", id);
        var command = new ResolveAlertCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        
        return NoContent();
    }
}

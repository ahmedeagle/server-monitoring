using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitoring.Application.DTOs;

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
        // TODO: Implement GetAlertsQuery
        return Ok(new List<AlertDto>());
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<List<AlertDto>>> GetServerAlerts(int serverId)
    {
        _logger.LogInformation("Getting alerts for server {ServerId}", serverId);
        // TODO: Implement GetServerAlertsQuery
        return Ok(new List<AlertDto>());
    }

    [HttpPost("{id}/acknowledge")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        _logger.LogInformation("Acknowledging alert {AlertId}", id);
        // TODO: Implement AcknowledgeAlertCommand
        return NoContent();
    }

    [HttpPost("{id}/resolve")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Resolve(int id)
    {
        _logger.LogInformation("Resolving alert {AlertId}", id);
        // TODO: Implement ResolveAlertCommand
        return NoContent();
    }
}

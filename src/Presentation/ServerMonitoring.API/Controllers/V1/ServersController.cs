using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.API.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ServersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ServersController> _logger;

    public ServersController(IMediator mediator, ILogger<ServersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServerDto>>> GetAll()
    {
        _logger.LogInformation("Getting all servers");
        // TODO: Implement GetAllServersQuery
        return Ok(new List<ServerDto>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerDto>> GetById(int id)
    {
        _logger.LogInformation("Getting server {ServerId}", id);
        // TODO: Implement GetServerByIdQuery
        return Ok(new ServerDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServerDto>> Create([FromBody] CreateServerDto dto)
    {
        _logger.LogInformation("Creating new server: {ServerName}", dto.Name);
        // TODO: Implement CreateServerCommand
        return CreatedAtAction(nameof(GetById), new { id = 1 }, new ServerDto());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServerDto>> Update(int id, [FromBody] UpdateServerDto dto)
    {
        _logger.LogInformation("Updating server {ServerId}", id);
        // TODO: Implement UpdateServerCommand
        return Ok(new ServerDto());
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting server {ServerId}", id);
        // TODO: Implement DeleteServerCommand
        return NoContent();
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Features.Servers.Commands;
using ServerMonitoring.Application.Features.Servers.Queries;

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
        var result = await _mediator.Send(new GetAllServersQuery());
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerDto>> GetById(int id)
    {
        _logger.LogInformation("Getting server {ServerId}", id);
        var result = await _mediator.Send(new GetServerByIdQuery { Id = id });
        
        if (!result.IsSuccess)
        {
            return NotFound(result.Message);
        }
        
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServerDto>> Create([FromBody] CreateServerDto dto)
    {
        _logger.LogInformation("Creating new server: {ServerName}", dto.Name);
        
        var command = new CreateServerCommand
        {
            Name = dto.Name,
            Hostname = dto.Hostname,
            IPAddress = dto.IpAddress,
            Port = dto.Port,
            OperatingSystem = dto.OperatingSystem
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServerDto>> Update(int id, [FromBody] UpdateServerDto dto)
    {
        _logger.LogInformation("Updating server {ServerId}", id);
        
        var command = new UpdateServerCommand
        {
            Id = id,
            Name = dto.Name,
            Hostname = dto.Hostname,
            IPAddress = dto.IpAddress,
            Port = dto.Port,
            OperatingSystem = dto.OperatingSystem
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.Message });
        }
        
        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting server {ServerId}", id);
        
        var command = new DeleteServerCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.Message });
        }
        
        return NoContent();
    }
}

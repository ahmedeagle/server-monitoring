using MediatR;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.Application.Features.Auth.Commands;

public record LoginCommand : IRequest<AuthResponseDto>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

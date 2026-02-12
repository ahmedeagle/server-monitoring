using MediatR;
using ServerMonitoring.Application.DTOs;

namespace ServerMonitoring.Application.Features.Auth.Commands;

public record RegisterUserCommand : IRequest<AuthResponseDto>
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}

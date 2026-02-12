using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.DTOs;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Entities;

namespace ServerMonitoring.Application.Features.Auth.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public RegisterUserCommandHandler(IApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password
        var passwordHash = _authService.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _context.Users.Add(user);

        // Assign default "User" role
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
        if (userRole != null)
        {
            var userRoleMapping = new UserRole
            {
                User = user,
                RoleId = userRole.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            _context.UserRoles.Add(userRoleMapping);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Get user roles
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        // Generate tokens
        var token = _authService.GenerateJwtToken(user.Id, user.Username, user.Email, roles);
        var refreshToken = _authService.GenerateRefreshToken();

        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Roles = roles
        };
    }
}

namespace ServerMonitoring.Application.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateJwtToken(int userId, string username, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
}

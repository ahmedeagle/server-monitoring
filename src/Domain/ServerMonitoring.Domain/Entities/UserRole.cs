using ServerMonitoring.Domain.Common;

namespace ServerMonitoring.Domain.Entities;

/// <summary>
/// Many-to-Many junction table between Users and Roles
/// </summary>
public class UserRole : IAuditable
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

namespace ServerMonitoring.Domain.Common;

/// <summary>
/// Interface for soft delete functionality
/// Marks records as deleted without physical deletion
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
    
    void Delete();
    void Restore();
}

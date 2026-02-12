using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using ServerMonitoring.Domain.Common;

namespace ServerMonitoring.Infrastructure.Interceptors;

/// <summary>
/// EF Core interceptor for automatic audit logging
/// Captures all changes (Insert, Update, Delete) with user and timestamp
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<AuditInterceptor> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(
        ILogger<AuditInterceptor> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AuditEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AuditEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditEntities(DbContext? context)
    {
        if (context == null) return;

        var currentUser = GetCurrentUser();
        var timestamp = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAt = timestamp;
                        auditable.CreatedBy = currentUser;
                        
                        // Get primary key value(s)
                        var addedKey = GetEntityKey(entry);
                        
                        _logger.LogInformation(
                            "Audit: Entity {EntityType} with ID {EntityId} CREATED by {User} at {Timestamp}",
                            entry.Entity.GetType().Name,
                            addedKey,
                            currentUser,
                            timestamp);
                        break;

                    case EntityState.Modified:
                        auditable.UpdatedAt = timestamp;
                        auditable.UpdatedBy = currentUser;
                        
                        var modifiedProperties = entry.Properties
                            .Where(p => p.IsModified)
                            .Select(p => new
                            {
                                Property = p.Metadata.Name,
                                OldValue = p.OriginalValue,
                                NewValue = p.CurrentValue
                            })
                            .ToList();
                        
                        // Get primary key value(s)
                        var modifiedKey = GetEntityKey(entry);

                        _logger.LogInformation(
                            "Audit: Entity {EntityType} with ID {EntityId} MODIFIED by {User} at {Timestamp}. Changed properties: {Properties}",
                            entry.Entity.GetType().Name,
                            modifiedKey,
                            currentUser,
                            timestamp,
                            modifiedProperties);
                        break;
                }
            }

            // Handle soft delete
            if (entry.Entity is ISoftDelete softDelete && entry.State == EntityState.Modified)
            {
                if (entry.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue as bool? == true &&
                    entry.Property(nameof(ISoftDelete.IsDeleted)).OriginalValue as bool? == false)
                {
                    softDelete.DeletedAt = timestamp;
                    softDelete.DeletedBy = currentUser;
                    
                    // Get primary key value(s)
                    var deletedKey = GetEntityKey(entry);
                    
                    _logger.LogWarning(
                        "Audit: Entity {EntityType} with ID {EntityId} SOFT DELETED by {User} at {Timestamp}",
                        entry.Entity.GetType().Name,
                        deletedKey,
                        currentUser,
                        timestamp);
                }
            }
        }
    }
    
    private static string GetEntityKey(EntityEntry entry)
    {
        // Get primary key properties
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        
        if (keyProperties == null || !keyProperties.Any())
            return "Unknown";
        
        // If single key property, return its value
        if (keyProperties.Count == 1)
        {
            var keyValue = entry.Property(keyProperties[0].Name).CurrentValue;
            return keyValue?.ToString() ?? "null";
        }
        
        // If composite key, return combined values
        var keyValues = keyProperties
            .Select(p => $"{p.Name}={entry.Property(p.Name).CurrentValue}")
            .ToArray();
        
        return string.Join(", ", keyValues);
    }

    private string GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User.Identity.Name ?? "System";
        }
        return "System";
    }
}

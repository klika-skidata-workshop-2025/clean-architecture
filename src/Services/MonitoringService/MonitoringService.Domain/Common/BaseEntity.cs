namespace MonitoringService.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides common properties like ID, timestamps, and update tracking.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public string Id { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the entity was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Marks the entity as updated by setting UpdatedAt to current UTC time.
    /// Call this method whenever you modify the entity.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

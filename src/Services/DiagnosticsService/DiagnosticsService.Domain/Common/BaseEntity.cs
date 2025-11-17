namespace DiagnosticsService.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    public string Id { get; protected set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

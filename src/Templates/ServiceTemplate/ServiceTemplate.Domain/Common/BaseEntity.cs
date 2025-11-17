namespace ServiceTemplate.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides common properties like Id and timestamps.
/// </summary>
/// <remarks>
/// Entities have identity and lifecycle.
/// Two entities are equal if they have the same Id, even if their properties differ.
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public string Id { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Marks the entity as updated by setting UpdatedAt to current time.
    /// Call this method whenever the entity is modified.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Entity equality is based on Id.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Hash code is based on Id.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}

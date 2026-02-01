namespace Legatro.DataLayer.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// The type of entity that was not found.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// The identifier of the entity that was not found.
    /// </summary>
    public object? EntityId { get; }

    public EntityNotFoundException(Type entityType, object? entityId)
        : base($"{entityType.Name} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(Type entityType, object? entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(Type entityType, object? entityId, string message, Exception innerException)
        : base(message, innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
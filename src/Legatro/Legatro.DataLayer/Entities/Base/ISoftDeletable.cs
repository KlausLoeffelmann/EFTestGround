namespace Legatro.DataLayer.Entities.Base;

/// <summary>
/// Interface for entities that support soft delete functionality.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// The date and time when the entity was soft-deleted (UTC).
    /// Null if the entity is not deleted.
    /// </summary>
    DateTime? IsDeleted { get; set; }
}

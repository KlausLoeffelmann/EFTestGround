namespace Legatro.DataLayer.Entities.Base;

/// <summary>
/// Base entity class that all entities inherit from.
/// Provides auditing properties for tracking creation and modification.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Synchronization GUID used for sync check with last changed date.
    /// Automatically regenerated on create and modify operations.
    /// </summary>
    public Guid SyncGuid { get; set; }

    /// <summary>
    /// The date and time when the entity was created (UTC).
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// The date and time when the entity was last edited (UTC).
    /// </summary>
    public DateTime DateLastEdited { get; set; }
}

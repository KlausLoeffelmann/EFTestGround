namespace Legatro.DataLayer.Entities;

/// <summary>
/// Base entity class that provides common fields for all entities in the system.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for synchronization purposes.
    /// </summary>
    public Guid SyncGuid { get; set; }

    /// <summary>
    /// The date and time when the entity was created.
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// The date and time when the entity was last edited.
    /// </summary>
    public DateTime DateLastEdited { get; set; }

    /// <summary>
    /// The date and time when the entity was soft deleted, or null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }
}
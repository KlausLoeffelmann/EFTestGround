using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a category for classifying TimeItems and Tasks.
/// </summary>
public class Category : BaseEntity, ISoftDeletable
{
    public Guid IdCategory { get; set; }

    public string CategoryName { get; set; } = null!;
    public string? CategoryDescription { get; set; }

    /// <summary>
    /// System categories cannot be deleted.
    /// </summary>
    public bool IsSystemCategory { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }
}

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a category for organizing TimeItems and Tasks.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdCategory { get; set; }

    /// <summary>
    /// Name of the category.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the category.
    /// </summary>
    public string? CategoryDescription { get; set; }

    /// <summary>
    /// Indicates whether this is a system category (cannot be deleted).
    /// </summary>
    public bool IsSystemCategory { get; set; }

    // Navigation properties (reverse)
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
}
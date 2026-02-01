using Legatro.DataLayer.Enums;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Defines the booking event types and their behavior for time tracking.
/// </summary>
public class TimeItemType : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdTimeItemType { get; set; }

    /// <summary>
    /// Name of the time item type.
    /// </summary>
    public string TimeItemTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Short name for display.
    /// </summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int? DisplayOrder { get; set; }

    /// <summary>
    /// Indicates whether this is a system type (cannot be deleted).
    /// </summary>
    public bool IsSystemType { get; set; }

    /// <summary>
    /// Booking type mapped to TimeItemBookingType enum.
    /// </summary>
    public short BookingType { get; set; }

    /// <summary>
    /// Description of the time item type.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // Navigation properties (reverse)
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
}
using Legatro.DataLayer.Entities.Base;
using Legatro.DataLayer.Enums;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Defines booking event types and their behavior.
/// Contains system-defined types that cannot be deleted.
/// </summary>
public class TimeItemType : BaseEntity, ISoftDeletable
{
    public Guid IdTimeItemType { get; set; }

    public string TimeItemTypeName { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public int? DisplayOrder { get; set; }

    /// <summary>
    /// System types cannot be deleted.
    /// </summary>
    public bool IsSystemType { get; set; }

    /// <summary>
    /// Maps to TimeItemBookingType enum for plausibility validation.
    /// </summary>
    public TimeItemBookingType BookingType { get; set; }

    public string Description { get; set; } = null!;

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }
}

/// <summary>
/// Well-known TimeItemType IDs for use in code.
/// </summary>
public static class TimeItemTypeIds
{
    public static readonly Guid Default = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid CheckIn = new("00000000-0000-0000-0000-000000000002");
    public static readonly Guid CheckOut = new("00000000-0000-0000-0000-000000000003");
    public static readonly Guid Break = new("00000000-0000-0000-0000-000000000004");
    public static readonly Guid Downtime = new("00000000-0000-0000-0000-000000000005");
    public static readonly Guid BusinessErrand = new("00000000-0000-0000-0000-000000000006");
    public static readonly Guid SetBooking = new("00000000-0000-0000-0000-000000000007");
}

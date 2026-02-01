using NetTopologySuite.Geometries;
using Legatro.DataLayer.Entities.Base;
using Legatro.DataLayer.Enums;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// The core time tracking entity representing a single point-in-time event.
/// Uses event-based recording where duration is calculated as delta to next/previous events.
/// Events are linked in a doubly-linked list per user per booking date.
/// </summary>
public class TimeItem : BaseEntity
{
    public Guid IdTimeItem { get; set; }

    #region Foreign Keys

    /// <summary>
    /// The user who created this time item.
    /// </summary>
    public Guid IdUser { get; set; }

    /// <summary>
    /// Optional project this time item is booked against.
    /// </summary>
    public Guid? IdProject { get; set; }

    /// <summary>
    /// Optional task this time item is booked against.
    /// </summary>
    public Guid? IdTask { get; set; }

    /// <summary>
    /// Optional category for classifying this time item.
    /// </summary>
    public Guid? IdTimeItemCategory { get; set; }

    /// <summary>
    /// The booking type (CheckIn, CheckOut, Break, etc.).
    /// </summary>
    public Guid? IdTimeItemType { get; set; }

    /// <summary>
    /// User who requested/assigned this item (for task assignments).
    /// </summary>
    public Guid? IdUserItemFrom { get; set; }

    #endregion

    #region Self-References (4 types)

    /// <summary>
    /// Reference to the archived version when this item is updated.
    /// Used for version history preservation - we never directly edit, we archive and create new.
    /// </summary>
    public Guid? IdHistoryParent { get; set; }

    /// <summary>
    /// Reference to the next item in the linked list for the same user/date.
    /// </summary>
    public Guid? IdNextItem { get; set; }

    /// <summary>
    /// Reference to the previous item in the linked list for the same user/date.
    /// </summary>
    public Guid? IdPreviousItem { get; set; }

    /// <summary>
    /// Reference to a parent time item (for hierarchical task relationships).
    /// </summary>
    public Guid? IdParentTimeItem { get; set; }

    #endregion

    #region Event Properties

    /// <summary>
    /// The type of event (Time, Task, Comment, Notification, etc.).
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// Debug/info message for the event.
    /// </summary>
    public string? EventInfo { get; set; }

    /// <summary>
    /// The time when this event occurred.
    /// </summary>
    public DateTimeOffset? EventTime { get; set; }

    /// <summary>
    /// The absolute booking date in GMT (time component is ignored).
    /// Used to group events by day.
    /// </summary>
    public DateTime? BookingDateGMT { get; set; }

    #endregion

    #region Duration Calculations

    /// <summary>
    /// Duration to the next event as TimeSpan.
    /// </summary>
    public TimeSpan? DurationToNext { get; set; }

    /// <summary>
    /// Duration to the next event in ticks (for SQL aggregation).
    /// </summary>
    public long? DurationTicksToNext { get; set; }

    /// <summary>
    /// Duration from the previous event as TimeSpan.
    /// </summary>
    public TimeSpan? DurationToPrevious { get; set; }

    /// <summary>
    /// Duration from the previous event in ticks (for SQL aggregation).
    /// </summary>
    public long? DurationTicksToPrevious { get; set; }

    #endregion

    #region Content Properties

    /// <summary>
    /// Short description of the time item.
    /// </summary>
    public string? ShortTitel { get; set; }

    /// <summary>
    /// Full description of the time item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Entity value or percentage complete.
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// Priority level (1=highest, 1000=standard/low).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Sort order for items with same priority/date.
    /// </summary>
    public double SortOrder { get; set; }

    /// <summary>
    /// Additional JSON metadata.
    /// </summary>
    public string? MetaInfo { get; set; }

    #endregion

    #region Status Flags

    /// <summary>
    /// Whether this item is marked as completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Whether this item is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Stops backward chain traversal when true.
    /// </summary>
    public bool IsStartAction { get; set; }

    /// <summary>
    /// Stops forward chain traversal when true.
    /// </summary>
    public bool IsEndAction { get; set; }

    /// <summary>
    /// Whether task assignment was rejected.
    /// </summary>
    public bool IsAssignmentRejected { get; set; }

    /// <summary>
    /// Quick task flag (may become obsolete).
    /// </summary>
    public bool IsNewQuickItem { get; set; }

    /// <summary>
    /// Whether tagged for due date notification.
    /// </summary>
    public bool IsTaggedForDueNotification { get; set; }

    #endregion

    #region Workflow/Assignment Properties

    /// <summary>
    /// Date when completion was requested.
    /// </summary>
    public DateTimeOffset? ItemCompletedRequestDate { get; set; }

    /// <summary>
    /// Date when assignment was accepted or rejected.
    /// </summary>
    public DateTimeOffset? DateItemAcceptedOrRejected { get; set; }

    /// <summary>
    /// Date when item was finished.
    /// </summary>
    public DateTimeOffset? DateItemFinished { get; set; }

    /// <summary>
    /// Date when last notification was sent.
    /// </summary>
    public DateTimeOffset? LastNotificationSentDate { get; set; }

    /// <summary>
    /// Date when notification was acknowledged.
    /// </summary>
    public DateTimeOffset? NotificationAcknowledgedDate { get; set; }

    #endregion

    #region Versioning

    /// <summary>
    /// When set, indicates this is an archived version valid until this date.
    /// Current/active versions have this as null.
    /// </summary>
    public DateTime? DateValidTo { get; set; }

    #endregion

    #region External Integration

    /// <summary>
    /// External reference ID for integration (MS To-Do, AzDO, Git, SAP, etc.).
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Device information for tracking origin.
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Location information as text.
    /// </summary>
    public string? LocationInfo { get; set; }

    /// <summary>
    /// Geospatial location data.
    /// </summary>
    public Point? Location { get; set; }

    #endregion

    #region Navigation Properties

    public virtual User User { get; set; } = null!;
    public virtual User? RequestingUser { get; set; }
    public virtual Project? Project { get; set; }
    public virtual LegatroTask? Task { get; set; }
    public virtual Category? Category { get; set; }
    public virtual TimeItemType? TimeItemType { get; set; }

    // Self-reference navigations
    public virtual TimeItem? HistoryParent { get; set; }
    public virtual ICollection<TimeItem> HistoryChildren { get; set; } = new List<TimeItem>();

    // Note: NextItem and PreviousItem navigation properties intentionally omitted
    // IdNextItem and IdPreviousItem are manually-managed linked list pointers (not FK relationships)

    public virtual TimeItem? ParentTimeItem { get; set; }
    public virtual ICollection<TimeItem> ChildTimeItems { get; set; } = new List<TimeItem>();

    #endregion
}

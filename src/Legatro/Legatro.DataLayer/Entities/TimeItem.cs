using Legatro.DataLayer.Enums;
using NetTopologySuite.Geometries;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a time tracking event. TimeItems are events, not time ranges.
/// Duration is calculated as the delta to the next/previous event.
/// </summary>
public class TimeItem
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdTimeItem { get; set; }

    /// <summary>
    /// User this time item belongs to.
    /// </summary>
    public Guid IdUser { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Project this time item is associated with.
    /// </summary>
    public Guid? IdProject { get; set; }

    /// <summary>
    /// Navigation property to the project.
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// Task this time item is associated with.
    /// </summary>
    public Guid? IdTask { get; set; }

    /// <summary>
    /// Navigation property to the task.
    /// </summary>
    public Task? Task { get; set; }

    /// <summary>
    /// Category this time item belongs to.
    /// </summary>
    public Guid? IdTimeItemCategory { get; set; }

    /// <summary>
    /// Navigation property to the category.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Type of booking event.
    /// </summary>
    public Guid? IdTimeItemType { get; set; }

    /// <summary>
    /// Navigation property to the time item type.
    /// </summary>
    public TimeItemType? TimeItemType { get; set; }

    /// <summary>
    /// Debug message for the event.
    /// </summary>
    public string? EventInfo { get; set; }

    /// <summary>
    /// Type of event.
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// Parent time item for history lineage.
    /// When this item is edited, the original is archived and linked here.
    /// </summary>
    public Guid? IdHistoryParent { get; set; }

    /// <summary>
    /// Navigation property to the history parent.
    /// </summary>
    public TimeItem? HistoryParent { get; set; }

    /// <summary>
    /// Navigation property to history versions.
    /// </summary>
    public virtual ICollection<TimeItem> HistoryVersions { get; set; } = new List<TimeItem>();

    /// <summary>
    /// Short description.
    /// </summary>
    public string? ShortTitel { get; set; }

    /// <summary>
    /// Full description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Link to the next item in the linked list.
    /// </summary>
    public Guid? IdNextItem { get; set; }

    /// <summary>
    /// Navigation property to the next item.
    /// </summary>
    public TimeItem? NextItem { get; set; }

    /// <summary>
    /// Navigation property to the previous item (reverse).
    /// </summary>
    public virtual ICollection<TimeItem> PreviousItems { get; set; } = new List<TimeItem>();

    /// <summary>
    /// Duration to the next item.
    /// </summary>
    public TimeSpan? DurationToNext { get; set; }

    /// <summary>
    /// Duration to the next item in ticks (for SQL aggregation).
    /// </summary>
    public long? DurationTicksToNext { get; set; }

    /// <summary>
    /// Link to the previous item in the linked list.
    /// </summary>
    public Guid? IdPreviousItem { get; set; }

    /// <summary>
    /// Navigation property to the previous item.
    /// </summary>
    public TimeItem? PreviousItem { get; set; }

    /// <summary>
    /// Duration from the previous item.
    /// </summary>
    public TimeSpan? DurationToPrevious { get; set; }

    /// <summary>
    /// Duration from the previous item in ticks (for SQL aggregation).
    /// </summary>
    public long? DurationTicksToPrevious { get; set; }

    /// <summary>
    /// Date when the item completion was requested.
    /// </summary>
    public DateTimeOffset? ItemCompletedRequestDate { get; set; }

    /// <summary>
    /// User who requested this item.
    /// </summary>
    public Guid? IdUserItemFrom { get; set; }

    /// <summary>
    /// Navigation property to the requesting user.
    /// </summary>
    public User? RequestedBy { get; set; }

    /// <summary>
    /// Date when the item was accepted or rejected.
    /// </summary>
    public DateTimeOffset? DateItemAcceptedOrRejected { get; set; }

    /// <summary>
    /// Date when the item was finished.
    /// </summary>
    public DateTimeOffset? DateItemFinished { get; set; }

    /// <summary>
    /// Parent time item for task hierarchy.
    /// </summary>
    public Guid? IdParentTimeItem { get; set; }

    /// <summary>
    /// Navigation property to the parent time item.
    /// </summary>
    public TimeItem? ParentTimeItem { get; set; }

    /// <summary>
    /// Navigation property to child time items.
    /// </summary>
    public virtual ICollection<TimeItem> ChildTimeItems { get; set; } = new List<TimeItem>();

    /// <summary>
    /// External reference ID for integration with external systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Date when the last notification was sent.
    /// </summary>
    public DateTimeOffset? LastNotificationSentDate { get; set; }

    /// <summary>
    /// Date when the notification was acknowledged.
    /// </summary>
    public DateTimeOffset? NotificationAcknowledgedDate { get; set; }

    /// <summary>
    /// Indicates whether this item is tagged for due notification.
    /// </summary>
    public bool IsTaggedForDueNotification { get; set; }

    /// <summary>
    /// Indicates whether this is a quick item.
    /// </summary>
    public bool IsNewQuickItem { get; set; }

    /// <summary>
    /// Indicates whether this item is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Indicates whether this item is soft deleted.
    /// Note: TimeItem uses bool IsDeleted, unlike other entities which use DateTime? IsDeleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Indicates whether this is a start action (stops backward chain traversal).
    /// </summary>
    public bool IsStartAction { get; set; }

    /// <summary>
    /// Indicates whether this is an end action (stops forward chain traversal).
    /// </summary>
    public bool IsEndAction { get; set; }

    /// <summary>
    /// Indicates whether the assignment was rejected.
    /// </summary>
    public bool IsAssignmentRejected { get; set; }

    /// <summary>
    /// Event time or task due time.
    /// </summary>
    public DateTimeOffset? EventTime { get; set; }

    /// <summary>
    /// Absolute day date in GMT.
    /// </summary>
    public DateTime? BookingDateGMT { get; set; }

    /// <summary>
    /// Entity value or percentage done.
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// Priority (1=highest, 1000=standard/low).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Sort order for items with the same priority/date.
    /// </summary>
    public double SortOrder { get; set; }

    /// <summary>
    /// Additional JSON metadata.
    /// </summary>
    public string? MetaInfo { get; set; }

    /// <summary>
    /// Device information.
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Location information.
    /// </summary>
    public string? LocationInfo { get; set; }

    /// <summary>
    /// Geographic location.
    /// </summary>
    public Point? Location { get; set; }

    /// <summary>
    /// Date when the item was last edited.
    /// </summary>
    public DateTime DateLastEdited { get; set; }

    /// <summary>
    /// Date when this version became invalid (null for current versions).
    /// Used to identify current vs historical records.
    /// </summary>
    public DateTime? DateValidTo { get; set; }

    /// <summary>
    /// Date when the item was created.
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Unique identifier for synchronization purposes.
    /// </summary>
    public Guid SyncGuid { get; set; }

    /// <summary>
    /// Navigation property to users who requested this item.
    /// </summary>
    public virtual ICollection<TimeItem> RequestedItems { get; set; } = new List<TimeItem>();
}
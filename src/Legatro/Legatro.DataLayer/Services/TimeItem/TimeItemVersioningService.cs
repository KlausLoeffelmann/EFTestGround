using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;

namespace Legatro.DataLayer.Services.TimeItem;

/// <summary>
/// Manages TimeItem version history.
/// Implements the pattern where updates create archived copies instead of direct edits.
/// </summary>
public class TimeItemVersioningService : ITimeItemVersioningService
{
    private readonly LegatroDbContext _context;
    private readonly ITimeItemLinkedListManager _linkedListManager;

    public TimeItemVersioningService(
        LegatroDbContext context,
        ITimeItemLinkedListManager linkedListManager)
    {
        _context = context;
        _linkedListManager = linkedListManager;
    }

    public async Task<(Entities.TimeItem Archived, Entities.TimeItem Updated)> CreateVersionAsync(
        Entities.TimeItem original,
        Action<Entities.TimeItem> updateAction,
        CancellationToken ct = default)
    {
        // Remember the old EventTime to check if we need to recalculate linked list
        var oldEventTime = original.EventTime;

        // 1. Create archive copy
        var archived = CloneForArchive(original);
        archived.IdTimeItem = Guid.NewGuid();
        archived.DateValidTo = DateTime.UtcNow;
        archived.IdHistoryParent = original.IdTimeItem;

        // Clear the archived copy's linked list pointers (it's no longer part of the chain)
        archived.IdNextItem = null;
        archived.IdPreviousItem = null;
        archived.DurationToNext = null;
        archived.DurationTicksToNext = null;
        archived.DurationToPrevious = null;
        archived.DurationTicksToPrevious = null;

        _context.TimeItems.Add(archived);

        // 2. Apply updates to original
        updateAction(original);

        // 3. If EventTime changed, we need to recalculate the linked list position
        if (oldEventTime != original.EventTime)
        {
            // Remove from current position
            await _linkedListManager.RemoveFromChainAsync(original, ct);

            // Re-insert at new position
            await _linkedListManager.InsertIntoChainAsync(original, ct);
        }

        return (archived, original);
    }

    public async Task<IReadOnlyList<Entities.TimeItem>> GetHistoryAsync(
        Guid currentItemId,
        CancellationToken ct = default)
    {
        var history = new List<Entities.TimeItem>();

        // Get all items that have this item as their history parent (archived versions)
        var archivedVersions = await _context.TimeItems
            .Where(t => t.IdHistoryParent == currentItemId)
            .OrderBy(t => t.DateValidTo)
            .ToListAsync(ct);

        history.AddRange(archivedVersions);

        // Also look for older versions recursively
        foreach (var archived in archivedVersions.ToList())
        {
            var olderVersions = await GetHistoryAsync(archived.IdTimeItem, ct);
            history.AddRange(olderVersions);
        }

        // Sort by DateValidTo (oldest first)
        return history.OrderBy(h => h.DateValidTo ?? DateTime.MaxValue).ToList();
    }

    /// <summary>
    /// Creates a deep copy of a TimeItem for archiving.
    /// </summary>
    private static Entities.TimeItem CloneForArchive(Entities.TimeItem source)
    {
        return new Entities.TimeItem
        {
            // Copy all properties
            IdUser = source.IdUser,
            IdProject = source.IdProject,
            IdTask = source.IdTask,
            IdTimeItemCategory = source.IdTimeItemCategory,
            IdTimeItemType = source.IdTimeItemType,
            IdUserItemFrom = source.IdUserItemFrom,
            IdParentTimeItem = source.IdParentTimeItem,

            EventType = source.EventType,
            EventInfo = source.EventInfo,
            EventTime = source.EventTime,
            BookingDateGMT = source.BookingDateGMT,

            ShortTitel = source.ShortTitel,
            Description = source.Description,
            Value = source.Value,
            Priority = source.Priority,
            SortOrder = source.SortOrder,
            MetaInfo = source.MetaInfo,

            IsCompleted = source.IsCompleted,
            IsDeleted = source.IsDeleted,
            IsStartAction = source.IsStartAction,
            IsEndAction = source.IsEndAction,
            IsAssignmentRejected = source.IsAssignmentRejected,
            IsNewQuickItem = source.IsNewQuickItem,
            IsTaggedForDueNotification = source.IsTaggedForDueNotification,

            ItemCompletedRequestDate = source.ItemCompletedRequestDate,
            DateItemAcceptedOrRejected = source.DateItemAcceptedOrRejected,
            DateItemFinished = source.DateItemFinished,
            LastNotificationSentDate = source.LastNotificationSentDate,
            NotificationAcknowledgedDate = source.NotificationAcknowledgedDate,

            ExternalReferenceId = source.ExternalReferenceId,
            DeviceInfo = source.DeviceInfo,
            LocationInfo = source.LocationInfo,
            Location = source.Location,

            // Copy base entity properties
            SyncGuid = source.SyncGuid,
            DateCreated = source.DateCreated,
            DateLastEdited = source.DateLastEdited
        };
    }
}

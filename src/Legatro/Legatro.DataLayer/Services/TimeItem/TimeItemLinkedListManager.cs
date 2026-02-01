using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;

namespace Legatro.DataLayer.Services.TimeItem;

/// <summary>
/// Manages the doubly-linked list structure of TimeItems.
/// Handles insertion, removal, and duration calculations.
/// </summary>
public class TimeItemLinkedListManager : ITimeItemLinkedListManager
{
    private readonly LegatroDbContext _context;

    public TimeItemLinkedListManager(LegatroDbContext context)
    {
        _context = context;
    }

    public async Task InsertIntoChainAsync(Entities.TimeItem newItem, CancellationToken ct = default)
    {
        if (!newItem.BookingDateGMT.HasValue || !newItem.EventTime.HasValue)
        {
            // Items without booking date or event time are not part of the chain
            return;
        }

        // 1. Find previous and next items
        var previous = await FindPreviousItemAsync(
            newItem.IdUser,
            newItem.BookingDateGMT.Value,
            newItem.EventTime.Value,
            newItem.IdTimeItem,
            ct);

        var next = await FindNextItemAsync(
            newItem.IdUser,
            newItem.BookingDateGMT.Value,
            newItem.EventTime.Value,
            newItem.IdTimeItem,
            ct);

        // 2. Link new item to its neighbors
        newItem.IdPreviousItem = previous?.IdTimeItem;
        newItem.IdNextItem = next?.IdTimeItem;

        // 3. Update previous item to point to new item
        if (previous != null)
        {
            previous.IdNextItem = newItem.IdTimeItem;
            RecalculateDurationToNext(previous, newItem);
        }

        // 4. Update next item to point to new item
        if (next != null)
        {
            next.IdPreviousItem = newItem.IdTimeItem;
            RecalculateDurationToPrevious(next, newItem);
        }

        // 5. Calculate durations for the new item
        if (previous != null)
        {
            RecalculateDurationToPrevious(newItem, previous);
        }
        else
        {
            newItem.DurationToPrevious = null;
            newItem.DurationTicksToPrevious = null;
        }

        if (next != null)
        {
            RecalculateDurationToNext(newItem, next);
        }
        else
        {
            newItem.DurationToNext = null;
            newItem.DurationTicksToNext = null;
        }
    }

    public async Task RemoveFromChainAsync(Entities.TimeItem item, CancellationToken ct = default)
    {
        // Find the actual previous and next items
        Entities.TimeItem? previous = null;
        Entities.TimeItem? next = null;

        if (item.IdPreviousItem.HasValue)
        {
            previous = await _context.TimeItems
                .FirstOrDefaultAsync(t => t.IdTimeItem == item.IdPreviousItem.Value, ct);
        }

        if (item.IdNextItem.HasValue)
        {
            next = await _context.TimeItems
                .FirstOrDefaultAsync(t => t.IdTimeItem == item.IdNextItem.Value, ct);
        }

        // Link previous directly to next (bypass the removed item)
        if (previous != null)
        {
            previous.IdNextItem = next?.IdTimeItem;
            if (next != null)
            {
                RecalculateDurationToNext(previous, next);
            }
            else
            {
                previous.DurationToNext = null;
                previous.DurationTicksToNext = null;
            }
        }

        if (next != null)
        {
            next.IdPreviousItem = previous?.IdTimeItem;
            if (previous != null)
            {
                RecalculateDurationToPrevious(next, previous);
            }
            else
            {
                next.DurationToPrevious = null;
                next.DurationTicksToPrevious = null;
            }
        }

        // Clear the removed item's links
        item.IdPreviousItem = null;
        item.IdNextItem = null;
        item.DurationToPrevious = null;
        item.DurationTicksToPrevious = null;
        item.DurationToNext = null;
        item.DurationTicksToNext = null;
    }

    public async Task<Entities.TimeItem?> FindPreviousItemAsync(
        Guid userId,
        DateTime bookingDate,
        DateTimeOffset eventTime,
        Guid? excludeItemId = null,
        CancellationToken ct = default)
    {
        var dateOnly = bookingDate.Date;

        // Load candidates and filter in memory due to SQLite DateTimeOffset translation issues
        var candidates = await _context.TimeItems
            .Where(t => t.IdUser == userId && t.BookingDateGMT == dateOnly)
            .ToListAsync(ct);

        var query = candidates
            .Where(t => t.EventTime < eventTime && !t.IsDeleted && !t.DateValidTo.HasValue);

        if (excludeItemId.HasValue)
        {
            query = query.Where(t => t.IdTimeItem != excludeItemId.Value);
        }

        return query
            .OrderByDescending(t => t.EventTime)
            .FirstOrDefault();
    }

    public async Task<Entities.TimeItem?> FindNextItemAsync(
        Guid userId,
        DateTime bookingDate,
        DateTimeOffset eventTime,
        Guid? excludeItemId = null,
        CancellationToken ct = default)
    {
        var dateOnly = bookingDate.Date;

        // Load candidates and filter in memory due to SQLite DateTimeOffset translation issues
        var candidates = await _context.TimeItems
            .Where(t => t.IdUser == userId && t.BookingDateGMT == dateOnly)
            .ToListAsync(ct);

        var query = candidates
            .Where(t => t.EventTime > eventTime && !t.IsDeleted && !t.DateValidTo.HasValue);

        if (excludeItemId.HasValue)
        {
            query = query.Where(t => t.IdTimeItem != excludeItemId.Value);
        }

        return query
            .OrderBy(t => t.EventTime)
            .FirstOrDefault();
    }

    public void RecalculateDurations(
        Entities.TimeItem item,
        Entities.TimeItem? previous,
        Entities.TimeItem? next)
    {
        if (previous != null)
        {
            RecalculateDurationToPrevious(item, previous);
        }
        else
        {
            item.DurationToPrevious = null;
            item.DurationTicksToPrevious = null;
        }

        if (next != null)
        {
            RecalculateDurationToNext(item, next);
        }
        else
        {
            item.DurationToNext = null;
            item.DurationTicksToNext = null;
        }
    }

    private static void RecalculateDurationToNext(Entities.TimeItem from, Entities.TimeItem to)
    {
        if (from.EventTime.HasValue && to.EventTime.HasValue)
        {
            var duration = to.EventTime.Value - from.EventTime.Value;
            from.DurationToNext = duration;
            from.DurationTicksToNext = duration.Ticks;
        }
        else
        {
            from.DurationToNext = null;
            from.DurationTicksToNext = null;
        }
    }

    private static void RecalculateDurationToPrevious(Entities.TimeItem to, Entities.TimeItem from)
    {
        if (from.EventTime.HasValue && to.EventTime.HasValue)
        {
            var duration = to.EventTime.Value - from.EventTime.Value;
            to.DurationToPrevious = duration;
            to.DurationTicksToPrevious = duration.Ticks;
        }
        else
        {
            to.DurationToPrevious = null;
            to.DurationTicksToPrevious = null;
        }
    }
}

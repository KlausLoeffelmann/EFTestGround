using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Exceptions;
using Legatro.DataLayer.Validators;
using Microsoft.EntityFrameworkCore;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.DataLayer.Services;

/// <summary>
/// Service for managing TimeItem entities with complex business logic.
/// </summary>
public class TimeItemService : ITimeItemService
{
    private readonly LegatroDbContext _context;
    private readonly TimeItemValidator _validator;
    private readonly IDbContextFactory<LegatroDbContext> _factory;

    public TimeItemService(
        LegatroDbContext context,
        TimeItemValidator validator,
        IDbContextFactory<LegatroDbContext> factory)
    {
        _context = context;
        _validator = validator;
        _factory = factory;
    }

    /// <inheritdoc />
    public async Task<TimeItem> CreateAsync(TimeItem timeItem, CancellationToken cancellationToken = default)
    {
        // Validate the time item
        var validationResult = await ValidateAsync(timeItem, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new TimeItemValidationException(
                typeof(TimeItem),
                timeItem.IdTimeItem,
                null,
                null,
                $"TimeItem validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Get the last time item for this user before this event
        var eventDateTime = timeItem.EventTime?.UtcDateTime ?? DateTime.UtcNow;
        var lastItem = await GetLastTimeItemBeforeAsync(
            timeItem.IdUser,
            eventDateTime,
            cancellationToken);

        // Set linked-list references
        if (lastItem != null)
        {
            timeItem.IdPreviousItem = lastItem.IdTimeItem;
            timeItem.DurationToPrevious = eventDateTime - lastItem.EventTime!.Value.UtcDateTime;
            timeItem.DurationTicksToPrevious = timeItem.DurationToPrevious.Value.Ticks;

            // Update the previous item to point to this new item
            lastItem.IdNextItem = timeItem.IdTimeItem;
            lastItem.DurationToNext = eventDateTime - lastItem.EventTime.Value.UtcDateTime;
            if (lastItem.DurationToNext.HasValue)
            {
                lastItem.DurationTicksToNext = lastItem.DurationToNext.Value.Ticks;
            }

            _context.TimeItems.Update(lastItem);
        }

        // Set audit fields
        timeItem.DateCreated = DateTime.UtcNow;
        timeItem.DateLastEdited = DateTime.UtcNow;
        timeItem.SyncGuid = Guid.NewGuid();

        _context.TimeItems.Add(timeItem);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get all navigation properties
        return await GetByIdAsync(timeItem.IdTimeItem, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(TimeItem), timeItem.IdTimeItem);
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task<TimeItem> UpdateAsync(TimeItem timeItem, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(timeItem.IdTimeItem, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(TimeItem), timeItem.IdTimeItem);

        // Validate the updated time item
        var validationResult = await ValidateAsync(timeItem, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new TimeItemValidationException(
                typeof(TimeItem),
                timeItem.IdTimeItem,
                null,
                null,
                $"TimeItem validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Create a historical version BEFORE updating the current version
        // Only create if this is not already a historical version (no IdHistoryParent)
        if (!existing.IdHistoryParent.HasValue)
        {
            // Fetch the current database values (untracked) to create historical version
            var currentDbValue = await _context.TimeItems
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.IdTimeItem == timeItem.IdTimeItem, cancellationToken);

            if (currentDbValue != null)
            {
                System.Diagnostics.Debug.WriteLine($"Creating historical version for {currentDbValue.IdTimeItem} with ShortTitel: '{currentDbValue.ShortTitel}'");
                await CreateHistoricalVersionAsync(currentDbValue, cancellationToken);
            }
        }

        // Update fields
        existing.IdProject = timeItem.IdProject;
        existing.IdTask = timeItem.IdTask;
        existing.IdTimeItemType = timeItem.IdTimeItemType;
        existing.EventType = timeItem.EventType;
        existing.ShortTitel = timeItem.ShortTitel;
        existing.EventTime = timeItem.EventTime;
        existing.BookingDateGMT = timeItem.BookingDateGMT;
        existing.IsCompleted = timeItem.IsCompleted;

        // Update linked-list references if event time changed
        if (existing.EventTime != timeItem.EventTime && existing.EventTime.HasValue && timeItem.EventTime.HasValue)
        {
            await UpdateLinkedListAsync(existing, timeItem.EventTime.Value.UtcDateTime, cancellationToken);
        }

        existing.DateLastEdited = DateTime.UtcNow;
        _context.TimeItems.Update(existing);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get updated navigation properties and duration fields
        _context.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        return await GetByIdAsync(timeItem.IdTimeItem, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(TimeItem), timeItem.IdTimeItem);
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var timeItem = await GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(TimeItem), id);

        // Create a historical version before deleting
        await CreateHistoricalVersionAsync(timeItem, cancellationToken);

        // Get linked items
        var previousItem = timeItem.IdPreviousItem.HasValue
            ? await GetByIdAsync(timeItem.IdPreviousItem.Value, cancellationToken)
            : null;

        var nextItem = timeItem.IdNextItem.HasValue
            ? await GetByIdAsync(timeItem.IdNextItem.Value, cancellationToken)
            : null;

        // Unlink and relink
        if (previousItem != null && nextItem != null)
        {
            // Link previous to next directly
            previousItem.IdNextItem = nextItem.IdTimeItem;
            previousItem.DurationToNext = nextItem.EventTime - previousItem.EventTime;
            if (previousItem.DurationToNext.HasValue)
            {
                previousItem.DurationTicksToNext = previousItem.DurationToNext.Value.Ticks;
            }

            nextItem.IdPreviousItem = previousItem.IdTimeItem;
            nextItem.DurationToPrevious = nextItem.EventTime - previousItem.EventTime;
            if (nextItem.DurationToPrevious.HasValue)
            {
                nextItem.DurationTicksToPrevious = nextItem.DurationToPrevious.Value.Ticks;
            }

            _context.TimeItems.Update(previousItem);
            _context.TimeItems.Update(nextItem);
        }
        else if (previousItem != null)
        {
            // Remove forward link from previous
            previousItem.IdNextItem = null;
            previousItem.DurationToNext = null;
            previousItem.DurationTicksToNext = null;

            _context.TimeItems.Update(previousItem);
        }
        else if (nextItem != null)
        {
            // Remove backward link from next
            nextItem.IdPreviousItem = null;
            nextItem.DurationToPrevious = null;
            nextItem.DurationTicksToPrevious = null;

            _context.TimeItems.Update(nextItem);
        }

        // Soft delete
        timeItem.IsDeleted = true;
        timeItem.DateLastEdited = DateTime.UtcNow;
        _context.TimeItems.Update(timeItem);

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TimeItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TimeItems
            .Include(t => t.User)
            .Include(t => t.Project)
            .Include(t => t.Task)
            .Include(t => t.TimeItemType)
            .FirstOrDefaultAsync(t => t.IdTimeItem == id && !t.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TimeItem>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // First get the current version (ignore query filter to get historical versions too)
        var current = await _context.TimeItems
            .IgnoreQueryFilters()
            .Include(t => t.User)
            .Include(t => t.Project)
            .Include(t => t.Task)
            .Include(t => t.TimeItemType)
            .FirstOrDefaultAsync(t => t.IdTimeItem == id, cancellationToken);

        if (current == null)
        {
            // Maybe this is a historical version, so find by IdHistoryParent
            current = await _context.TimeItems
                .IgnoreQueryFilters()
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Task)
                .Include(t => t.TimeItemType)
                .Where(t => t.IdHistoryParent == id)
                .OrderByDescending(t => t.DateLastEdited)
                .FirstOrDefaultAsync(cancellationToken);

            if (current == null)
            {
                return new List<TimeItem>();
            }

            id = current.IdHistoryParent ?? Guid.Empty;
        }

        // Get all historical versions (ignore query filter to include historical versions)
        var history = await _context.TimeItems
            .IgnoreQueryFilters()
            .Include(t => t.User)
            .Include(t => t.Project)
            .Include(t => t.Task)
            .Include(t => t.TimeItemType)
            .Where(t => t.IdTimeItem == id || t.IdHistoryParent == id)
            .OrderBy(t => t.DateLastEdited)
            .ToListAsync(cancellationToken);

        return history;
    }

    /// <inheritdoc />
    public async Task<List<TimeItem>> GetByUserAndDateAsync(
        Guid userId,
        DateTime date,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TimeItems
            .Include(t => t.User)
            .Include(t => t.Project)
            .Include(t => t.Task)
            .Include(t => t.TimeItemType)
            .Where(t => t.IdUser == userId && t.BookingDateGMT == date);

        if (!includeDeleted)
        {
            query = query.Where(t => !t.IsDeleted);
        }

        return await query
            .OrderBy(t => t.EventTime)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TimeItem>> GetChainAsync(
        Guid startItemId,
        ChainDirection direction = ChainDirection.Forward,
        CancellationToken cancellationToken = default)
    {
        var chain = new List<TimeItem>();
        var startItem = await GetByIdAsync(startItemId, cancellationToken);

        if (startItem == null)
        {
            return chain;
        }

        if (direction == ChainDirection.Backward || direction == ChainDirection.Both)
        {
            // Traverse backward
            var currentItem = startItem;
            while (currentItem != null && currentItem.IdPreviousItem.HasValue)
            {
                var previousItem = await GetByIdAsync(currentItem.IdPreviousItem.Value, cancellationToken);
                if (previousItem == null)
                {
                    break;
                }
                chain.Insert(0, previousItem);
                currentItem = previousItem;
            }
        }

        chain.Add(startItem);

        if (direction == ChainDirection.Forward || direction == ChainDirection.Both)
        {
            // Traverse forward
            var currentItem = startItem;
            while (currentItem != null && currentItem.IdNextItem.HasValue)
            {
                var nextItem = await GetByIdAsync(currentItem.IdNextItem.Value, cancellationToken);
                if (nextItem == null)
                {
                    break;
                }
                chain.Add(nextItem);
                currentItem = nextItem;
            }
        }

        return chain;
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task<TimeSpan> GetTotalBookedTimeAsync(
        Guid userId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var timeItems = await GetByUserAndDateAsync(userId, date, false, cancellationToken);

        if (timeItems.Count == 0)
        {
            return TimeSpan.Zero;
        }

        // Items are already ordered by EventTime
        long totalTicks = 0;
        long breakTicks = 0;

        for (int i = 0; i < timeItems.Count; i++)
        {
            var item = timeItems[i];

            // Add all durations to total
            if (item.DurationToNext.HasValue)
            {
                totalTicks += item.DurationToNext.Value.Ticks;
            }

            // If this item is a Break, subtract the time it takes (DurationToPrevious)
            if (item.TimeItemType?.BookingType == (short)TimeItemBookingType.Break &&
                item.DurationToPrevious.HasValue)
            {
                breakTicks += item.DurationToPrevious.Value.Ticks;
            }
        }

        return TimeSpan.FromTicks(totalTicks - breakTicks);
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task<TimeItemValidationResult> ValidateAsync(
        TimeItem timeItem,
        CancellationToken cancellationToken = default)
    {
        var result = new TimeItemValidationResult { IsValid = true };

        // Additional business rules
        if (timeItem.EventTime.HasValue && timeItem.EventTime.Value.UtcDateTime.Kind != DateTimeKind.Utc)
        {
            result.Errors.Add("EventTime must be in UTC.");
            result.IsValid = false;
        }

        if (timeItem.BookingDateGMT != (timeItem.EventTime?.UtcDateTime.Date ?? DateTime.UtcNow.Date))
        {
            result.Warnings.Add("BookingDateGMT does not match EventTime.Date");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TimeItem?> GetLastTimeItemBeforeAsync(
        Guid userId,
        DateTime before,
        CancellationToken cancellationToken = default)
    {
        return await _context.TimeItems
            .Where(t => t.IdUser == userId
                && t.EventTime < before
                && !t.IsDeleted)
            .OrderByDescending(t => t.EventTime)
            .Include(t => t.TimeItemType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task RecalculateChainDurationsAsync(
        Guid startItemId,
        CancellationToken cancellationToken = default)
    {
        var chain = await GetChainAsync(startItemId, ChainDirection.Backward, cancellationToken);

        if (chain.Count == 0)
        {
            return;
        }

        // The first item in the chain (chronologically) has no previous
        for (int i = 0; i < chain.Count; i++)
        {
            var current = chain[i];

            if (i > 0)
            {
                var previous = chain[i - 1];
                current.IdPreviousItem = previous.IdTimeItem;
                current.DurationToPrevious = current.EventTime - previous.EventTime;
                if (current.DurationToPrevious.HasValue)
                {
                    current.DurationTicksToPrevious = current.DurationToPrevious.Value.Ticks;
                }
            }
            else
            {
                current.IdPreviousItem = null;
                current.DurationToPrevious = null;
                current.DurationTicksToPrevious = null;
            }

            if (i < chain.Count - 1)
            {
                var next = chain[i + 1];
                current.IdNextItem = next.IdTimeItem;
                current.DurationToNext = next.EventTime - current.EventTime;
                if (current.DurationToNext.HasValue)
                {
                    current.DurationTicksToNext = current.DurationToNext.Value.Ticks;
                }
            }
            else
            {
                current.IdNextItem = null;
                current.DurationToNext = null;
                current.DurationTicksToNext = null;
            }

            current.DateLastEdited = DateTime.UtcNow;
            _context.TimeItems.Update(current);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a historical version of a TimeItem.
    /// Uses a fresh context to avoid relationship tracking issues.
    /// </summary>
    private async System.Threading.Tasks.Task CreateHistoricalVersionAsync(TimeItem timeItem, CancellationToken cancellationToken)
    {
        var historyId = Guid.NewGuid();
        var dateValidTo = DateTime.UtcNow;

        // Clone the time item - only copy scalar properties, not navigation properties
        var history = new TimeItem
        {
            IdTimeItem = historyId,
            IdUser = timeItem.IdUser,
            IdProject = timeItem.IdProject,
            IdTask = timeItem.IdTask,
            IdTimeItemType = timeItem.IdTimeItemType,
            EventType = timeItem.EventType,
            ShortTitel = timeItem.ShortTitel,
            EventTime = timeItem.EventTime,
            BookingDateGMT = timeItem.BookingDateGMT,
            IsCompleted = timeItem.IsCompleted,
            IsDeleted = false,  // Historical versions are not soft-deleted, they're inactive via DateValidTo

            // Linked-list references are NOT copied to history
            IdPreviousItem = null,
            IdNextItem = null,
            DurationToPrevious = null,
            DurationToNext = null,
            DurationTicksToPrevious = null,
            DurationTicksToNext = null,

            // Versioning
            IdHistoryParent = timeItem.IdTimeItem,
            DateValidTo = dateValidTo,

            // Audit fields
            DateCreated = timeItem.DateCreated,
            DateLastEdited = timeItem.DateLastEdited,
            SyncGuid = Guid.NewGuid()  // Unique SyncGuid for historical versions
        };

        // Use a fresh context to avoid tracking conflicts
        using var freshContext = _factory.CreateDbContext();
        freshContext.TimeItems.Add(history);
        await freshContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates linked-list references when event time changes.
    /// </summary>
    private async System.Threading.Tasks.Task UpdateLinkedListAsync(
        TimeItem timeItem,
        DateTime newEventTime,
        CancellationToken cancellationToken)
    {
        // Get previous and next items using AsNoTracking to avoid tracking conflicts
        var previousItem = timeItem.IdPreviousItem.HasValue
            ? await _context.TimeItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTimeItem == timeItem.IdPreviousItem.Value, cancellationToken)
            : null;

        var nextItem = timeItem.IdNextItem.HasValue
            ? await _context.TimeItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTimeItem == timeItem.IdNextItem.Value, cancellationToken)
            : null;

        // Update previous item's forward link
        if (previousItem != null)
        {
            previousItem.DurationToNext = new DateTimeOffset(newEventTime) - previousItem.EventTime;
            if (previousItem.DurationToNext.HasValue)
            {
                previousItem.DurationTicksToNext = previousItem.DurationToNext.Value.Ticks;
            }
            _context.TimeItems.Update(previousItem);
        }

        // Update next item's backward link
        if (nextItem != null)
        {
            nextItem.DurationToPrevious = nextItem.EventTime - new DateTimeOffset(newEventTime);
            if (nextItem.DurationToPrevious.HasValue)
            {
                nextItem.DurationTicksToPrevious = nextItem.DurationToPrevious.Value.Ticks;
            }
            _context.TimeItems.Update(nextItem);
        }

        // Update current item's backward link
        if (previousItem != null)
        {
            timeItem.DurationToPrevious = new DateTimeOffset(newEventTime) - previousItem.EventTime;
            if (timeItem.DurationToPrevious.HasValue)
            {
                timeItem.DurationTicksToPrevious = timeItem.DurationToPrevious.Value.Ticks;
            }
        }

        // Update current item's forward link
        if (nextItem != null)
        {
            timeItem.DurationToNext = nextItem.EventTime - new DateTimeOffset(newEventTime);
            if (timeItem.DurationToNext.HasValue)
            {
                timeItem.DurationTicksToNext = timeItem.DurationToNext.Value.Ticks;
            }
        }
    }
}
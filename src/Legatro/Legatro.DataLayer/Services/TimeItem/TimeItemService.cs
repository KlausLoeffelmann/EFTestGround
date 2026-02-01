using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Services.Interfaces;
using Legatro.DataLayer.Validators;

namespace Legatro.DataLayer.Services.TimeItem;

/// <summary>
/// Main service for TimeItem operations with full business logic.
/// Coordinates validation, linked-list maintenance, and versioning.
/// </summary>
public class TimeItemService : ITimeItemService
{
    private readonly LegatroDbContext _context;
    private readonly ITimeItemValidator _validator;
    private readonly ITimeItemLinkedListManager _linkedListManager;
    private readonly ITimeItemVersioningService _versioningService;

    public TimeItemService(
        LegatroDbContext context,
        ITimeItemValidator validator,
        ITimeItemLinkedListManager linkedListManager,
        ITimeItemVersioningService versioningService)
    {
        _context = context;
        _validator = validator;
        _linkedListManager = linkedListManager;
        _versioningService = versioningService;
    }

    public async Task<Entities.TimeItem> CreateAsync(Entities.TimeItem item, CancellationToken ct = default)
    {
        if (!item.BookingDateGMT.HasValue)
        {
            throw new TimeItemValidationException("BookingDateGMT is required for TimeItems.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            // 1. Get last event for validation
            var lastEvent = await GetLastEventAsync(item.IdUser, item.BookingDateGMT.Value, ct);

            // 2. Validate the new event
            var validationResult = await _validator.ValidateAsync(item, lastEvent, ct);
            if (!validationResult.IsValid)
            {
                throw new TimeItemValidationException(validationResult.ErrorMessage!);
            }

            // 3. Auto-insert CheckIn if required
            if (validationResult.RequiresAutoInsertCheckIn)
            {
                var autoCheckIn = CreateAutoCheckIn(item);
                _context.TimeItems.Add(autoCheckIn);
                await _linkedListManager.InsertIntoChainAsync(autoCheckIn, ct);
            }

            // 4. Ensure item has an ID
            if (item.IdTimeItem == Guid.Empty)
            {
                item.IdTimeItem = Guid.NewGuid();
            }

            // 5. Add the item and insert into linked list
            _context.TimeItems.Add(item);
            await _linkedListManager.InsertIntoChainAsync(item, ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return item;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Entities.TimeItem> UpdateAsync(Guid id, Action<Entities.TimeItem> updateAction, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var item = await _context.TimeItems
                .FirstOrDefaultAsync(t => t.IdTimeItem == id && !t.IsDeleted && !t.DateValidTo.HasValue, ct);

            if (item == null)
            {
                throw new TimeItemValidationException($"TimeItem with ID {id} not found or is deleted/archived.");
            }

            // Create version and apply updates
            var (_, updated) = await _versioningService.CreateVersionAsync(item, updateAction, ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return updated;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var item = await _context.TimeItems
                .FirstOrDefaultAsync(t => t.IdTimeItem == id && !t.IsDeleted && !t.DateValidTo.HasValue, ct);

            if (item == null)
            {
                throw new TimeItemValidationException($"TimeItem with ID {id} not found or is already deleted.");
            }

            // Remove from linked list first
            await _linkedListManager.RemoveFromChainAsync(item, ct);

            // Soft delete
            item.IsDeleted = true;

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<Entities.TimeItem>> GetHistoryAsync(Guid id, CancellationToken ct = default)
    {
        return await _versioningService.GetHistoryAsync(id, ct);
    }

    public async Task<Entities.TimeItem?> GetLastEventAsync(Guid userId, DateTime bookingDate, CancellationToken ct = default)
    {
        var dateOnly = bookingDate.Date;
        return await _context.TimeItems
            .Where(t => t.IdUser == userId
                && t.BookingDateGMT == dateOnly
                && !t.IsDeleted
                && !t.DateValidTo.HasValue)
            .OrderByDescending(t => t.EventTime)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Entities.TimeItem>> GetDayEventsAsync(Guid userId, DateTime bookingDate, CancellationToken ct = default)
    {
        var dateOnly = bookingDate.Date;
        return await _context.TimeItems
            .Include(t => t.TimeItemType)
            .Include(t => t.Project)
            .Include(t => t.Task)
            .Where(t => t.IdUser == userId
                && t.BookingDateGMT == dateOnly
                && !t.IsDeleted
                && !t.DateValidTo.HasValue)
            .OrderBy(t => t.EventTime)
            .ToListAsync(ct);
    }

    public async Task<Entities.TimeItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TimeItems
            .Include(t => t.TimeItemType)
            .Include(t => t.Project)
            .Include(t => t.Task)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.IdTimeItem == id, ct);
    }

    /// <summary>
    /// Creates an automatic CheckIn event based on the provided item.
    /// </summary>
    private static Entities.TimeItem CreateAutoCheckIn(Entities.TimeItem referenceItem)
    {
        // Calculate CheckIn time - 1 second before the reference event
        var checkInTime = referenceItem.EventTime?.AddSeconds(-1) ?? DateTimeOffset.UtcNow;

        return new Entities.TimeItem
        {
            IdTimeItem = Guid.NewGuid(),
            IdUser = referenceItem.IdUser,
            IdTimeItemType = TimeItemTypeIds.CheckIn,
            EventType = EventType.Time,
            EventTime = checkInTime,
            BookingDateGMT = referenceItem.BookingDateGMT,
            ShortTitel = "Auto CheckIn",
            EventInfo = "Automatically inserted CheckIn",
            Priority = 1000,
            SortOrder = 0
        };
    }
}

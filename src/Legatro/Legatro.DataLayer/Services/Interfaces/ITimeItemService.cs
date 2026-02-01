namespace Legatro.DataLayer.Services.Interfaces;

/// <summary>
/// Service interface for managing TimeItems with full business logic.
/// </summary>
public interface ITimeItemService
{
    /// <summary>
    /// Creates a new TimeItem with plausibility validation and linked-list maintenance.
    /// May auto-insert prerequisite events (e.g., CheckIn before Break).
    /// </summary>
    Task<Entities.TimeItem> CreateAsync(Entities.TimeItem item, CancellationToken ct = default);

    /// <summary>
    /// Updates a TimeItem by creating an archived version and applying changes.
    /// All changes are versioned - the original is never directly modified.
    /// </summary>
    Task<Entities.TimeItem> UpdateAsync(Guid id, Action<Entities.TimeItem> updateAction, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a TimeItem and updates the linked list.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the complete version history for a TimeItem.
    /// </summary>
    Task<IReadOnlyList<Entities.TimeItem>> GetHistoryAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the last event for a user on a specific booking date.
    /// </summary>
    Task<Entities.TimeItem?> GetLastEventAsync(Guid userId, DateTime bookingDate, CancellationToken ct = default);

    /// <summary>
    /// Gets all events for a user on a specific booking date in chronological order.
    /// </summary>
    Task<IReadOnlyList<Entities.TimeItem>> GetDayEventsAsync(Guid userId, DateTime bookingDate, CancellationToken ct = default);

    /// <summary>
    /// Gets a TimeItem by ID.
    /// </summary>
    Task<Entities.TimeItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

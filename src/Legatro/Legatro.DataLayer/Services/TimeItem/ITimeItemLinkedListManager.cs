using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Services.TimeItem;

/// <summary>
/// Interface for managing the doubly-linked list structure of TimeItems.
/// </summary>
public interface ITimeItemLinkedListManager
{
    /// <summary>
    /// Inserts a new TimeItem into the linked list chain for its user/date.
    /// Updates adjacent items' pointers and recalculates durations.
    /// </summary>
    Task InsertIntoChainAsync(Entities.TimeItem newItem, CancellationToken ct = default);

    /// <summary>
    /// Removes a TimeItem from the linked list chain.
    /// Links previous directly to next and recalculates durations.
    /// </summary>
    Task RemoveFromChainAsync(Entities.TimeItem item, CancellationToken ct = default);

    /// <summary>
    /// Finds the previous item in the chain based on EventTime.
    /// </summary>
    Task<Entities.TimeItem?> FindPreviousItemAsync(
        Guid userId,
        DateTime bookingDate,
        DateTimeOffset eventTime,
        Guid? excludeItemId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Finds the next item in the chain based on EventTime.
    /// </summary>
    Task<Entities.TimeItem?> FindNextItemAsync(
        Guid userId,
        DateTime bookingDate,
        DateTimeOffset eventTime,
        Guid? excludeItemId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Recalculates durations for an item based on its neighbors.
    /// </summary>
    void RecalculateDurations(Entities.TimeItem item, Entities.TimeItem? previous, Entities.TimeItem? next);
}

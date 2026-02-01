namespace Legatro.DataLayer.Services.TimeItem;

/// <summary>
/// Interface for managing TimeItem version history.
/// TimeItems are never directly modified - changes create archived copies.
/// </summary>
public interface ITimeItemVersioningService
{
    /// <summary>
    /// Creates a new version of a TimeItem by archiving the current state
    /// and applying the update action.
    /// </summary>
    /// <param name="original">The original TimeItem to version.</param>
    /// <param name="updateAction">Action to apply the updates.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tuple of (archived copy, updated original).</returns>
    Task<(Entities.TimeItem Archived, Entities.TimeItem Updated)> CreateVersionAsync(
        Entities.TimeItem original,
        Action<Entities.TimeItem> updateAction,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all historical versions of a TimeItem.
    /// </summary>
    /// <param name="currentItemId">The ID of the current TimeItem.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of all versions in chronological order (oldest first).</returns>
    Task<IReadOnlyList<Entities.TimeItem>> GetHistoryAsync(
        Guid currentItemId,
        CancellationToken ct = default);
}

using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using System.Threading.Tasks;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.DataLayer.Services;

/// <summary>
/// Service for managing TimeItem entities with complex business logic.
/// </summary>
public interface ITimeItemService
{
    /// <summary>
    /// Creates a new TimeItem with validation and linked-list maintenance.
    /// </summary>
    /// <param name="timeItem">The TimeItem to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created TimeItem with linked-list relationships established.</returns>
    Task<TimeItem> CreateAsync(TimeItem timeItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing TimeItem with validation and linked-list maintenance.
    /// </summary>
    /// <param name="timeItem">The TimeItem to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated TimeItem.</returns>
    Task<TimeItem> UpdateAsync(TimeItem timeItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a TimeItem (soft delete) with linked-list maintenance.
    /// </summary>
    /// <param name="id">The ID of the TimeItem to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    System.Threading.Tasks.Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a TimeItem by ID.
    /// </summary>
    /// <param name="id">The TimeItem ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The TimeItem, or null if not found.</returns>
    Task<TimeItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the history of a TimeItem (including all versions).
    /// </summary>
    /// <param name="id">The TimeItem ID (can be current or historical version).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All versions of the TimeItem.</returns>
    Task<List<TimeItem>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all TimeItems for a user on a specific date.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="date">The date to query.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The TimeItems for the user on the date.</returns>
    Task<List<TimeItem>> GetByUserAndDateAsync(
        Guid userId,
        DateTime date,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the linked chain of TimeItems starting from a given item.
    /// </summary>
    /// <param name="startItemId">The starting TimeItem ID.</param>
    /// <param name="direction">The direction to traverse (Forward or Backward).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chain of TimeItems.</returns>
    Task<List<TimeItem>> GetChainAsync(
        Guid startItemId,
        ChainDirection direction = ChainDirection.Forward,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total booked time for a user on a specific date.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="date">The date to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total booked time.</returns>
    Task<TimeSpan> GetTotalBookedTimeAsync(
        Guid userId,
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a TimeItem for business rule compliance.
    /// </summary>
    /// <param name="timeItem">The TimeItem to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A validation result with any errors.</returns>
    Task<TimeItemValidationResult> ValidateAsync(
        TimeItem timeItem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last TimeItem for a user before a specific date/time.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="before">The cutoff date/time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The last TimeItem, or null if none found.</returns>
    Task<TimeItem?> GetLastTimeItemBeforeAsync(
        Guid userId,
        DateTime before,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates all durations in a TimeItem chain.
    /// </summary>
    /// <param name="startItemId">The starting TimeItem ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    System.Threading.Tasks.Task RecalculateChainDurationsAsync(
        Guid startItemId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Direction to traverse a TimeItem chain.
/// </summary>
public enum ChainDirection
{
    /// <summary>
    /// Traverse forward through IdNextItem links.
    /// </summary>
    Forward,

    /// <summary>
    /// Traverse backward through IdPreviousItem links.
    /// </summary>
    Backward,

    /// <summary>
    /// Traverse both directions (centered on the start item).
    /// </summary>
    Both
}

/// <summary>
/// Result of TimeItem validation.
/// </summary>
public class TimeItemValidationResult
{
    /// <summary>
    /// Whether the TimeItem is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Collection of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Any warnings (non-blocking issues).
    /// </summary>
    public List<string> Warnings { get; set; } = new List<string>();
}
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;

namespace Legatro.DataLayer.Validators;

/// <summary>
/// Result of TimeItem validation including any required auto-insertions.
/// </summary>
public class TimeItemValidationResult
{
    public bool IsValid { get; init; }
    public bool RequiresAutoInsertCheckIn { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeItemBookingType? LastEventType { get; init; }
    public TimeItemBookingType NewEventType { get; init; }

    public static TimeItemValidationResult Success(TimeItemBookingType newEventType) =>
        new() { IsValid = true, NewEventType = newEventType };

    public static TimeItemValidationResult SuccessWithAutoCheckIn(TimeItemBookingType newEventType) =>
        new() { IsValid = true, RequiresAutoInsertCheckIn = true, NewEventType = newEventType };

    public static TimeItemValidationResult Failure(string errorMessage,
        TimeItemBookingType? lastEventType, TimeItemBookingType newEventType) =>
        new()
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            LastEventType = lastEventType,
            NewEventType = newEventType
        };
}

/// <summary>
/// Interface for validating TimeItem booking plausibility.
/// </summary>
public interface ITimeItemValidator
{
    /// <summary>
    /// Validates a new TimeItem against the current state for the user/date.
    /// </summary>
    /// <param name="newItem">The new TimeItem to validate.</param>
    /// <param name="lastEvent">The last event for the same user/date, or null if no events exist.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Validation result indicating if the operation is allowed and any auto-insertions required.</returns>
    Task<TimeItemValidationResult> ValidateAsync(
        TimeItem newItem,
        TimeItem? lastEvent,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the booking type for a TimeItem based on its TimeItemType.
    /// </summary>
    TimeItemBookingType GetBookingType(TimeItem item);
}

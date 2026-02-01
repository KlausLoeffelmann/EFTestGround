using Legatro.DataLayer.Enums;

namespace Legatro.DataLayer.Exceptions;

/// <summary>
/// Exception thrown when TimeItem validation fails.
/// </summary>
public class TimeItemValidationException : Exception
{
    /// <summary>
    /// The time item that failed validation.
    /// </summary>
    public Entities.TimeItem? TimeItem { get; }

    /// <summary>
    /// The expected booking type that was required.
    /// </summary>
    public TimeItemBookingType? ExpectedBookingType { get; }

    /// <summary>
    /// The actual booking type that was provided.
    /// </summary>
    public TimeItemBookingType? ActualBookingType { get; }

    /// <summary>
    /// The booking type of the last event in the sequence.
    /// </summary>
    public TimeItemBookingType? LastEventBookingType { get; }

    /// <summary>
    /// Whether auto-insertion is required.
    /// </summary>
    public bool RequiresAutoInsert { get; }

    /// <summary>
    /// The booking type that needs to be auto-inserted.
    /// </summary>
    public TimeItemBookingType? AutoInsertType { get; }

    /// <summary>
    /// The entity type (for compatibility with EntityNotFoundException pattern).
    /// </summary>
    public Type? EntityType { get; }

    /// <summary>
    /// The entity ID (for compatibility with EntityNotFoundException pattern).
    /// </summary>
    public object? EntityId { get; }

    public TimeItemValidationException(string message)
        : base(message)
    {
    }

    public TimeItemValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public TimeItemValidationException(
        string message,
        Entities.TimeItem? timeItem,
        TimeItemBookingType? expectedType,
        TimeItemBookingType? actualType,
        TimeItemBookingType? lastEventType)
        : base(message)
    {
        TimeItem = timeItem;
        ExpectedBookingType = expectedType;
        ActualBookingType = actualType;
        LastEventBookingType = lastEventType;
    }

    public TimeItemValidationException(
        string message,
        TimeItemBookingType requiredAutoInsertType)
        : base(message)
    {
        RequiresAutoInsert = true;
        AutoInsertType = requiredAutoInsertType;
    }

    /// <summary>
    /// Constructor for compatibility with EntityNotFoundException pattern.
    /// </summary>
    public TimeItemValidationException(
        Type entityType,
        Guid entityId,
        TimeItemBookingType? expectedType,
        TimeItemBookingType? actualType,
        string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
        ExpectedBookingType = expectedType;
        ActualBookingType = actualType;
    }
}
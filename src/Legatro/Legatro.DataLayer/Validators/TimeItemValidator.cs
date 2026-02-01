using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Legatro.DataLayer.Validators;

/// <summary>
/// Validates TimeItem operations according to business rules.
/// </summary>
public class TimeItemValidator
{
    private readonly LegatroDbContext _context;

    public TimeItemValidator(LegatroDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validates a TimeItem before insertion and determines if auto-insert is required.
    /// </summary>
    /// <param name="timeItem">The time item to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result indicating if auto-insert is required</returns>
    public async Task<(bool IsValid, TimeItemBookingType? AutoInsertType)> ValidateTimeItemInsertAsync(
        TimeItem timeItem,
        CancellationToken cancellationToken = default)
    {
        if (timeItem.IdTimeItemType == null)
        {
            // No time item type specified, allow
            return (true, null);
        }

        var timeItemType = await _context.TimeItemTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdTimeItemType == timeItem.IdTimeItemType, cancellationToken);

        if (timeItemType == null)
        {
            throw new EntityNotFoundException(typeof(TimeItemType), timeItem.IdTimeItemType);
        }

        var bookingType = (TimeItemBookingType)timeItemType.BookingType;

        // Get the last event for the user on the same booking date
        var lastEvent = await GetLastEventForUserAndDateAsync(
            timeItem.IdUser,
            timeItem.BookingDateGMT,
            cancellationToken);

        // Validate according to plausibility rules
        return await ValidateBookingTypeAsync(bookingType, lastEvent, cancellationToken);
    }

    /// <summary>
    /// Validates a booking type against the last event in the sequence.
    /// </summary>
    private async System.Threading.Tasks.Task<(bool IsValid, TimeItemBookingType? AutoInsertType)> ValidateBookingTypeAsync(
        TimeItemBookingType newBookingType,
        TimeItem? lastEvent,
        CancellationToken cancellationToken)
    {
        // No events exist yet
        if (lastEvent == null)
        {
            switch (newBookingType)
            {
                case TimeItemBookingType.CheckIn:
                    return (true, null);
                case TimeItemBookingType.CheckOut:
                    throw new TimeItemValidationException(
                        "First event of the day must be CheckIn, not CheckOut.",
                        null,
                        TimeItemBookingType.CheckIn,
                        TimeItemBookingType.CheckOut,
                        null);
                case TimeItemBookingType.Break:
                case TimeItemBookingType.Downtime:
                case TimeItemBookingType.BusinessErrand:
                case TimeItemBookingType.SetBooking:
                    return (false, TimeItemBookingType.CheckIn); // Auto-insert CheckIn first
                case TimeItemBookingType.Default:
                    return (true, null);
                default:
                    throw new TimeItemValidationException($"Unknown booking type: {newBookingType}");
            }
        }

        // Get the last event's booking type
        var lastBookingType = await GetBookingTypeAsync(lastEvent, cancellationToken);

        // Validate against the last event's booking type
        if (newBookingType == TimeItemBookingType.CheckIn && lastBookingType == TimeItemBookingType.CheckOut)
        {
            return (true, null);
        }

        if (newBookingType == TimeItemBookingType.CheckOut)
        {
            if (lastBookingType == TimeItemBookingType.CheckIn
                || lastBookingType == TimeItemBookingType.Break
                || lastBookingType == TimeItemBookingType.Downtime
                || lastBookingType == TimeItemBookingType.BusinessErrand
                || lastBookingType == TimeItemBookingType.SetBooking)
            {
                return (true, null);
            }

            if (lastBookingType == TimeItemBookingType.CheckOut)
            {
                throw new TimeItemValidationException(
                    "Cannot CheckOut after CheckOut. Must CheckIn first.",
                    null,
                    TimeItemBookingType.CheckIn,
                    TimeItemBookingType.CheckOut,
                    lastBookingType);
            }
        }

        if (newBookingType == TimeItemBookingType.Break)
        {
            if (lastBookingType == TimeItemBookingType.CheckIn || lastBookingType == TimeItemBookingType.SetBooking)
            {
                return (true, null);
            }

            if (lastBookingType == TimeItemBookingType.CheckOut
                || lastBookingType == TimeItemBookingType.Break
                || lastBookingType == TimeItemBookingType.Downtime
                || lastBookingType == TimeItemBookingType.BusinessErrand)
            {
                throw new TimeItemValidationException(
                    $"Cannot add Break after {lastBookingType}. Must CheckIn first.",
                    null,
                    TimeItemBookingType.CheckIn,
                    TimeItemBookingType.Break,
                    lastBookingType);
            }
        }

        if (newBookingType == TimeItemBookingType.Downtime)
        {
            if (lastBookingType == TimeItemBookingType.CheckIn || lastBookingType == TimeItemBookingType.SetBooking)
            {
                return (true, null);
            }

            if (lastBookingType == TimeItemBookingType.CheckOut
                || lastBookingType == TimeItemBookingType.Break
                || lastBookingType == TimeItemBookingType.Downtime
                || lastBookingType == TimeItemBookingType.BusinessErrand)
            {
                throw new TimeItemValidationException(
                    $"Cannot add Downtime after {lastBookingType}. Must CheckIn first.",
                    null,
                    TimeItemBookingType.CheckIn,
                    TimeItemBookingType.Downtime,
                    lastBookingType);
            }
        }

        if (newBookingType == TimeItemBookingType.BusinessErrand)
        {
            if (lastBookingType == TimeItemBookingType.CheckIn || lastBookingType == TimeItemBookingType.SetBooking)
            {
                return (true, null);
            }

            if (lastBookingType == TimeItemBookingType.CheckOut
                || lastBookingType == TimeItemBookingType.Break
                || lastBookingType == TimeItemBookingType.Downtime
                || lastBookingType == TimeItemBookingType.BusinessErrand)
            {
                throw new TimeItemValidationException(
                    $"Cannot add BusinessErrand after {lastBookingType}. Must CheckIn first.",
                    null,
                    TimeItemBookingType.CheckIn,
                    TimeItemBookingType.BusinessErrand,
                    lastBookingType);
            }
        }

        if (newBookingType == TimeItemBookingType.SetBooking)
        {
            if (lastBookingType == TimeItemBookingType.CheckIn
                || lastBookingType == TimeItemBookingType.CheckOut
                || lastBookingType == TimeItemBookingType.Break
                || lastBookingType == TimeItemBookingType.Downtime
                || lastBookingType == TimeItemBookingType.BusinessErrand
                || lastBookingType == TimeItemBookingType.SetBooking)
            {
                return (true, null);
            }
        }

        if (newBookingType == TimeItemBookingType.Default)
        {
            return (true, null);
        }

        throw new TimeItemValidationException($"Unknown booking type: {newBookingType}");
    }

    /// <summary>
    /// Gets the booking type for a TimeItem.
    /// </summary>
    private async System.Threading.Tasks.Task<TimeItemBookingType> GetBookingTypeAsync(TimeItem timeItem, CancellationToken cancellationToken)
    {
        if (timeItem.IdTimeItemType == null)
        {
            return TimeItemBookingType.Default;
        }

        // Try to use loaded navigation property first
        if (timeItem.TimeItemType != null)
        {
            return (TimeItemBookingType)timeItem.TimeItemType.BookingType;
        }

        // Otherwise fetch from database
        var timeItemType = await _context.TimeItemTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdTimeItemType == timeItem.IdTimeItemType, cancellationToken);

        if (timeItemType == null)
        {
            return TimeItemBookingType.Default;
        }

        return (TimeItemBookingType)timeItemType.BookingType;
    }

    /// <summary>
    /// Gets the last event for a user on a specific booking date.
    /// </summary>
    public async Task<TimeItem?> GetLastEventForUserAndDateAsync(
        Guid userId,
        DateTime? bookingDateGmt,
        CancellationToken cancellationToken = default)
    {
        return await _context.TimeItems
            .AsNoTracking()
            .Where(t => t.IdUser == userId
                && t.BookingDateGMT == bookingDateGmt
                && !t.IsDeleted)
            .OrderByDescending(t => t.EventTime)
            .ThenByDescending(t => t.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;

namespace Legatro.DataLayer.Validators;

/// <summary>
/// Validates TimeItem booking plausibility based on state machine rules.
/// </summary>
public class TimeItemValidator : ITimeItemValidator
{
    private readonly LegatroDbContext _context;

    /// <summary>
    /// Defines allowed transitions from each booking type.
    /// </summary>
    private static readonly Dictionary<TimeItemBookingType, HashSet<TimeItemBookingType>> AllowedTransitions = new()
    {
        // After CheckIn: anything except CheckIn
        {
            TimeItemBookingType.CheckIn, new HashSet<TimeItemBookingType>
            {
                TimeItemBookingType.CheckOut,
                TimeItemBookingType.Break,
                TimeItemBookingType.Downtime,
                TimeItemBookingType.BusinessErrand,
                TimeItemBookingType.SetBooking
            }
        },

        // After CheckOut: only CheckIn
        {
            TimeItemBookingType.CheckOut, new HashSet<TimeItemBookingType>
            {
                TimeItemBookingType.CheckIn
            }
        },

        // After Break: CheckIn (resume) or CheckOut
        {
            TimeItemBookingType.Break, new HashSet<TimeItemBookingType>
            {
                TimeItemBookingType.CheckIn,
                TimeItemBookingType.CheckOut
            }
        },

        // After Downtime: CheckIn (resume) or CheckOut
        {
            TimeItemBookingType.Downtime, new HashSet<TimeItemBookingType>
            {
                TimeItemBookingType.CheckIn,
                TimeItemBookingType.CheckOut
            }
        },

        // After BusinessErrand: CheckIn (resume) or CheckOut
        {
            TimeItemBookingType.BusinessErrand, new HashSet<TimeItemBookingType>
            {
                TimeItemBookingType.CheckIn,
                TimeItemBookingType.CheckOut
            }
        },

        // After SetBooking: same as after CheckIn (still working)
        {
            TimeItemBookingType.SetBooking, new HashSet<TimeItemBookingType>
            {
                TimeItemBookingType.CheckOut,
                TimeItemBookingType.Break,
                TimeItemBookingType.Downtime,
                TimeItemBookingType.BusinessErrand,
                TimeItemBookingType.SetBooking
            }
        },

        // Default type allows any transition
        {
            TimeItemBookingType.Default, new HashSet<TimeItemBookingType>
            {
                TimeItemBookingType.Default,
                TimeItemBookingType.CheckIn,
                TimeItemBookingType.CheckOut,
                TimeItemBookingType.Break,
                TimeItemBookingType.Downtime,
                TimeItemBookingType.BusinessErrand,
                TimeItemBookingType.SetBooking
            }
        }
    };

    public TimeItemValidator(LegatroDbContext context)
    {
        _context = context;
    }

    public Task<TimeItemValidationResult> ValidateAsync(
        TimeItem newItem,
        TimeItem? lastEvent,
        CancellationToken ct = default)
    {
        var newBookingType = GetBookingType(newItem);

        // No events today - special handling
        if (lastEvent == null)
        {
            return Task.FromResult(ValidateFirstEventOfDay(newBookingType));
        }

        var lastBookingType = GetBookingType(lastEvent);

        // Check allowed transitions
        if (AllowedTransitions.TryGetValue(lastBookingType, out var allowed))
        {
            if (!allowed.Contains(newBookingType))
            {
                return Task.FromResult(TimeItemValidationResult.Failure(
                    $"Cannot transition from {lastBookingType} to {newBookingType}. " +
                    $"Allowed after {lastBookingType}: {string.Join(", ", allowed)}",
                    lastBookingType,
                    newBookingType));
            }
        }

        return Task.FromResult(TimeItemValidationResult.Success(newBookingType));
    }

    public TimeItemBookingType GetBookingType(TimeItem item)
    {
        if (item.IdTimeItemType == null)
        {
            return TimeItemBookingType.Default;
        }

        // Use cached TimeItemType if loaded
        if (item.TimeItemType != null)
        {
            return item.TimeItemType.BookingType;
        }

        // Look up from well-known IDs
        if (item.IdTimeItemType == TimeItemTypeIds.Default)
            return TimeItemBookingType.Default;
        if (item.IdTimeItemType == TimeItemTypeIds.CheckIn)
            return TimeItemBookingType.CheckIn;
        if (item.IdTimeItemType == TimeItemTypeIds.CheckOut)
            return TimeItemBookingType.CheckOut;
        if (item.IdTimeItemType == TimeItemTypeIds.Break)
            return TimeItemBookingType.Break;
        if (item.IdTimeItemType == TimeItemTypeIds.Downtime)
            return TimeItemBookingType.Downtime;
        if (item.IdTimeItemType == TimeItemTypeIds.BusinessErrand)
            return TimeItemBookingType.BusinessErrand;
        if (item.IdTimeItemType == TimeItemTypeIds.SetBooking)
            return TimeItemBookingType.SetBooking;

        // Fall back to database lookup
        var timeItemType = _context.TimeItemTypes
            .AsNoTracking()
            .FirstOrDefault(t => t.IdTimeItemType == item.IdTimeItemType);

        return timeItemType?.BookingType ?? TimeItemBookingType.Default;
    }

    private static TimeItemValidationResult ValidateFirstEventOfDay(TimeItemBookingType newType)
    {
        return newType switch
        {
            // CheckIn is always valid as first event
            TimeItemBookingType.CheckIn => TimeItemValidationResult.Success(newType),

            // CheckOut is NOT valid as first event
            TimeItemBookingType.CheckOut => TimeItemValidationResult.Failure(
                "Cannot CheckOut as the first event of the day. You must CheckIn first.",
                null,
                newType),

            // Break, Downtime, BusinessErrand, SetBooking - auto-insert CheckIn
            TimeItemBookingType.Break or
            TimeItemBookingType.Downtime or
            TimeItemBookingType.BusinessErrand or
            TimeItemBookingType.SetBooking => TimeItemValidationResult.SuccessWithAutoCheckIn(newType),

            // Default type is always valid
            TimeItemBookingType.Default => TimeItemValidationResult.Success(newType),

            // Unknown type - allow with warning
            _ => TimeItemValidationResult.Success(newType)
        };
    }
}

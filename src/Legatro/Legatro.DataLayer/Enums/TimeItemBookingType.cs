namespace Legatro.DataLayer.Enums;

/// <summary>
/// Defines the type of booking event for time tracking.
/// </summary>
public enum TimeItemBookingType : short
{
    /// <summary>
    /// Default booking type
    /// </summary>
    Default = 0,

    /// <summary>
    /// Check-in event (start of work period)
    /// </summary>
    CheckIn = 1,

    /// <summary>
    /// Check-out event (end of work period)
    /// </summary>
    CheckOut = 2,

    /// <summary>
    /// Break event (short pause)
    /// </summary>
    Break = 3,

    /// <summary>
    /// Downtime event (non-productive time, e.g., waiting for deployment)
    /// </summary>
    Downtime = 4,

    /// <summary>
    /// Business errand event (client visits, meetings)
    /// </summary>
    BusinessErrand = 5,

    /// <summary>
    /// Set booking event (assign work to project/task)
    /// </summary>
    SetBooking = 6
}
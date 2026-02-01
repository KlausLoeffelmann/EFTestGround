namespace Legatro.DataLayer.Enums;

/// <summary>
/// Defines the booking type categories for TimeItemTypes.
/// </summary>
public enum TimeItemBookingType : short
{
    /// <summary>
    /// Default/Log entry - used for quantity capture or other non-personnel time tracking events.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Check-in - employee clocks in.
    /// </summary>
    CheckIn = 1,

    /// <summary>
    /// Check-out - employee clocks out.
    /// </summary>
    CheckOut = 2,

    /// <summary>
    /// Break - unspecified break.
    /// </summary>
    Break = 3,

    /// <summary>
    /// Downtime - booked when employee cannot continue working for operational reasons.
    /// </summary>
    Downtime = 4,

    /// <summary>
    /// Business errand - unspecified business errand outside the office.
    /// </summary>
    BusinessErrand = 5,

    /// <summary>
    /// Set booking - booking to a target set that can contain multiple projects, orders, products, or combinations.
    /// </summary>
    SetBooking = 6
}

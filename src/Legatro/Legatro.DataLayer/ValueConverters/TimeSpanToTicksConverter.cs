using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Legatro.DataLayer.ValueConverters;

/// <summary>
/// Converts TimeSpan to ticks (long) for SQL aggregation compatibility.
/// </summary>
public class TimeSpanToTicksConverter : ValueConverter<TimeSpan?, long?>
{
    public TimeSpanToTicksConverter() : base(
        v => v.HasValue ? v.Value.Ticks : null,
        v => v.HasValue ? TimeSpan.FromTicks(v.Value) : null)
    {
    }
}

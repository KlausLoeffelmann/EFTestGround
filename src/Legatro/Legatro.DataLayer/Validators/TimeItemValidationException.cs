namespace Legatro.DataLayer.Validators;

/// <summary>
/// Exception thrown when TimeItem validation fails.
/// </summary>
public class TimeItemValidationException : Exception
{
    public TimeItemValidationException(string message) : base(message)
    {
    }

    public TimeItemValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

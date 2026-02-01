namespace LegatroTestConsole.Commands;

/// <summary>
/// Interface for command handlers.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// The command path (e.g., ["time", "checkin"]).
    /// </summary>
    string[] CommandPath { get; }

    /// <summary>
    /// Short description of the command.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Execute the command.
    /// </summary>
    Task<int> ExecuteAsync(string[] args, CancellationToken ct = default);

    /// <summary>
    /// Print help for this command.
    /// </summary>
    void PrintHelp();
}

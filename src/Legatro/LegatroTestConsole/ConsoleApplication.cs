using Spectre.Console;
using LegatroTestConsole.Commands;

namespace LegatroTestConsole;

/// <summary>
/// Main interactive console application.
/// </summary>
public class ConsoleApplication
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<string, ICommandHandler> _handlers;

    public ConsoleApplication(IServiceProvider services, IEnumerable<ICommandHandler> handlers)
    {
        _services = services;
        _handlers = new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase);

        foreach (var handler in handlers)
        {
            var key = string.Join(" ", handler.CommandPath);
            _handlers[key] = handler;
        }
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        AnsiConsole.Write(new FigletText("Legatro").Color(Color.Blue));
        AnsiConsole.MarkupLine("[grey]Time Tracking & Project Management Console[/]");
        AnsiConsole.MarkupLine("[grey]Type 'help' for available commands, 'exit' to quit.[/]\n");

        while (!ct.IsCancellationRequested)
        {
            AnsiConsole.Markup("[green]legatro>[/] ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            var parts = ParseCommandLine(input);
            if (parts.Length == 0)
                continue;

            if (parts[0].Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                parts[0].Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[grey]Goodbye![/]");
                break;
            }

            await ExecuteCommandAsync(parts, ct);
        }
    }

    private async Task ExecuteCommandAsync(string[] parts, CancellationToken ct)
    {
        // Try to find a matching command handler
        ICommandHandler? handler = null;
        var matchLength = 0;

        // Try matching from longest to shortest command path
        for (var len = Math.Min(parts.Length, 3); len >= 1; len--)
        {
            var key = string.Join(" ", parts.Take(len));
            if (_handlers.TryGetValue(key, out handler))
            {
                matchLength = len;
                break;
            }
        }

        if (handler == null)
        {
            // Check if it's a category request
            var category = parts[0].ToLower();
            var categoryHandlers = _handlers.Values
                .Where(h => h.CommandPath.Length > 0 &&
                            h.CommandPath[0].Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (categoryHandlers.Any())
            {
                AnsiConsole.MarkupLine($"\n[blue]{category}[/] commands:\n");
                foreach (var h in categoryHandlers.OrderBy(h => string.Join(" ", h.CommandPath)))
                {
                    var cmd = string.Join(" ", h.CommandPath);
                    AnsiConsole.MarkupLine($"  {cmd,-25} - {h.Description}");
                }
                AnsiConsole.WriteLine();
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Unknown command: {parts[0]}[/]");
                AnsiConsole.MarkupLine("[grey]Type 'help' for available commands.[/]");
            }
            return;
        }

        // Check for help request
        if (parts.Length > matchLength &&
            parts[matchLength].Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            handler.PrintHelp();
            return;
        }

        // Execute the command
        var args = parts.Skip(matchLength).ToArray();

        try
        {
            var result = await handler.ExecuteAsync(args, ct);
            if (result != 0)
            {
                AnsiConsole.MarkupLine($"[yellow]Command returned exit code: {result}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
#if DEBUG
            AnsiConsole.WriteException(ex);
#endif
        }
    }

    private static string[] ParseCommandLine(string input)
    {
        var parts = new List<string>();
        var current = "";
        var inQuotes = false;

        foreach (var c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (!string.IsNullOrEmpty(current))
                {
                    parts.Add(current);
                    current = "";
                }
            }
            else
            {
                current += c;
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            parts.Add(current);
        }

        return parts.ToArray();
    }
}

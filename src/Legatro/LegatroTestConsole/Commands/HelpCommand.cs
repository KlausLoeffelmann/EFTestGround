using Spectre.Console;

namespace LegatroTestConsole.Commands;

public class HelpCommand : ICommandHandler
{
    private readonly IEnumerable<ICommandHandler> _handlers;

    public string[] CommandPath => ["help"];
    public string Description => "Show available commands";

    public HelpCommand(IEnumerable<ICommandHandler> handlers)
    {
        _handlers = handlers;
    }

    public Task<int> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        PrintHelp();
        return Task.FromResult(0);
    }

    public void PrintHelp()
    {
        AnsiConsole.MarkupLine("\n[bold]Available Commands:[/]\n");

        var grouped = _handlers
            .Where(h => h.CommandPath.Length > 0)
            .GroupBy(h => h.CommandPath[0])
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            AnsiConsole.MarkupLine($"[blue]{group.Key}[/]");
            foreach (var handler in group.OrderBy(h => string.Join(" ", h.CommandPath)))
            {
                var cmd = string.Join(" ", handler.CommandPath);
                AnsiConsole.MarkupLine($"  {cmd,-25} - {handler.Description}");
            }
            AnsiConsole.WriteLine();
        }

        AnsiConsole.MarkupLine("[grey]Type '<command> help' for detailed usage.[/]");
        AnsiConsole.MarkupLine("[grey]Type 'exit' to quit.[/]");
    }
}

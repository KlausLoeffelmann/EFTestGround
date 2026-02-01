using Spectre.Console;
using Legatro.DataLayer.DemoData;

namespace LegatroTestConsole.Commands;

public class DbRegenerateCommand : ICommandHandler
{
    private readonly IDemoDatabaseGenerator _generator;

    public string[] CommandPath => ["db", "regenerate"];
    public string Description => "Regenerate the demo database";

    public DbRegenerateCommand(IDemoDatabaseGenerator generator)
    {
        _generator = generator;
    }

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        var weeksStr = GetArgValue(args, "--weeks") ?? "4";
        if (!int.TryParse(weeksStr, out var weeks))
        {
            weeks = 4;
        }

        AnsiConsole.MarkupLine($"[yellow]Regenerating database with {weeks} weeks of data...[/]");

        await AnsiConsole.Status()
            .StartAsync("Generating demo data...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);

                await _generator.RegenerateAsync(new DemoDataConfiguration
                {
                    TimeDataWeeks = weeks,
                    StartDate = DateTime.UtcNow.AddDays(-weeks * 7)
                }, ct);
            });

        AnsiConsole.MarkupLine("[green]Database regenerated successfully![/]");
        AnsiConsole.MarkupLine($"[grey]Database path: {_generator.GetDatabasePath()}[/]");

        return 0;
    }

    public void PrintHelp()
    {
        AnsiConsole.MarkupLine("Usage: db regenerate [--weeks <n>]");
        AnsiConsole.MarkupLine("Clears and regenerates the demo database.");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("Options:");
        AnsiConsole.MarkupLine("  --weeks    Number of weeks of time data to generate (default: 4)");
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }
        return null;
    }
}

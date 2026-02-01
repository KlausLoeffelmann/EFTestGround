using Spectre.Console;
using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;
using Legatro.DataLayer.Services.Interfaces;

namespace LegatroTestConsole.Commands;

public class UserListCommand : ICommandHandler
{
    private readonly LegatroDbContext _context;

    public string[] CommandPath => ["user", "list"];
    public string Description => "List all users";

    public UserListCommand(LegatroDbContext context)
    {
        _context = context;
    }

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        var users = await _context.Users
            .Include(u => u.Contact)
            .Where(u => u.IsDeleted == null)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);

        if (!users.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No users found. Run 'db regenerate' to create demo data.[/]");
            return 0;
        }

        var table = new Table();
        table.AddColumn("Username");
        table.AddColumn("Name");
        table.AddColumn("Email");
        table.AddColumn("Active");
        table.AddColumn("Admin");

        foreach (var user in users)
        {
            table.AddRow(
                user.Username,
                $"{user.FirstName} {user.LastName}",
                user.Contact?.Email ?? "-",
                user.IsActivated ? "[green]Yes[/]" : "[red]No[/]",
                user.IsAdmin ? "[blue]Yes[/]" : "No"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[grey]Total: {users.Count} users[/]");

        return 0;
    }

    public void PrintHelp()
    {
        AnsiConsole.MarkupLine("Usage: user list");
        AnsiConsole.MarkupLine("Lists all active users in the system.");
    }
}

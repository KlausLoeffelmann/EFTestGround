using Spectre.Console;
using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;

namespace LegatroTestConsole.Commands;

public class ProjectListCommand : ICommandHandler
{
    private readonly LegatroDbContext _context;

    public string[] CommandPath => ["project", "list"];
    public string Description => "List all projects";

    public ProjectListCommand(LegatroDbContext context)
    {
        _context = context;
    }

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        var projects = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Customer)
            .Where(p => p.IsDeleted == null)
            .OrderBy(p => p.ProjectNumber)
            .ToListAsync(ct);

        if (!projects.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No projects found. Run 'db regenerate' to create demo data.[/]");
            return 0;
        }

        var table = new Table();
        table.AddColumn("#");
        table.AddColumn("Name");
        table.AddColumn("Owner");
        table.AddColumn("Customer");
        table.AddColumn("Active");

        foreach (var project in projects)
        {
            table.AddRow(
                project.ProjectNumber.ToString(),
                project.ProjectName,
                project.Owner != null ? $"{project.Owner.FirstName} {project.Owner.LastName}" : "-",
                project.Customer?.CompanyName ?? "-",
                project.IsActive ? "[green]Yes[/]" : "[red]No[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[grey]Total: {projects.Count} projects[/]");

        return 0;
    }

    public void PrintHelp()
    {
        AnsiConsole.MarkupLine("Usage: project list [--user <username>]");
        AnsiConsole.MarkupLine("Lists all active projects in the system.");
    }
}

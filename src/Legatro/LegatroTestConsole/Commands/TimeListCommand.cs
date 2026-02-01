using Spectre.Console;
using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Services.Interfaces;

namespace LegatroTestConsole.Commands;

public class TimeListCommand : ICommandHandler
{
    private readonly LegatroDbContext _context;
    private readonly ITimeItemService _timeItemService;

    public string[] CommandPath => ["time", "list"];
    public string Description => "List time items for a user/date";

    public TimeListCommand(LegatroDbContext context, ITimeItemService timeItemService)
    {
        _context = context;
        _timeItemService = timeItemService;
    }

    public async Task<int> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        // Parse arguments
        var username = GetArgValue(args, "--user") ?? GetArgValue(args, "-u");
        var dateStr = GetArgValue(args, "--date") ?? GetArgValue(args, "-d");

        if (string.IsNullOrEmpty(username))
        {
            AnsiConsole.MarkupLine("[red]Error: --user is required[/]");
            PrintHelp();
            return 1;
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsDeleted == null, ct);

        if (user == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: User '{username}' not found[/]");
            return 1;
        }

        var date = string.IsNullOrEmpty(dateStr)
            ? DateTime.UtcNow.Date
            : DateTime.Parse(dateStr).Date;

        var events = await _timeItemService.GetDayEventsAsync(user.IdUser, date, ct);

        if (!events.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No time items found for {username} on {date:yyyy-MM-dd}[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n[bold]Daily Time Report: {username} - {date:dddd, MMMM d, yyyy}[/]");
        AnsiConsole.MarkupLine(new string('=', 60));

        TimeSpan totalWork = TimeSpan.Zero;
        TimeSpan totalBreak = TimeSpan.Zero;

        foreach (var item in events)
        {
            var time = item.EventTime?.ToString("HH:mm") ?? "--:--";
            var typeShort = item.TimeItemType?.ShortName ?? "?";
            var title = item.ShortTitel ?? "";

            var durationStr = "";
            if (item.DurationToNext.HasValue && item.DurationToNext.Value.TotalMinutes >= 1)
            {
                var d = item.DurationToNext.Value;
                durationStr = d.TotalHours >= 1
                    ? $"{(int)d.TotalHours}h {d.Minutes}m"
                    : $"{d.Minutes}m";

                // Track totals
                var bookingType = item.TimeItemType?.BookingType ?? TimeItemBookingType.Default;
                if (bookingType == TimeItemBookingType.Break)
                    totalBreak += d;
                else if (bookingType is TimeItemBookingType.CheckIn or TimeItemBookingType.SetBooking)
                    totalWork += d;
            }

            var color = item.TimeItemType?.BookingType switch
            {
                TimeItemBookingType.CheckIn => "green",
                TimeItemBookingType.CheckOut => "red",
                TimeItemBookingType.Break => "yellow",
                TimeItemBookingType.SetBooking => "blue",
                _ => "white"
            };

            AnsiConsole.MarkupLine($"[{color}]{time}[/]  [{color}]{typeShort,-10}[/] {title}");
            if (!string.IsNullOrEmpty(durationStr))
            {
                AnsiConsole.MarkupLine($"       [grey]Duration: {durationStr}[/]");
            }

            if (item.Project != null)
            {
                AnsiConsole.MarkupLine($"       [grey]-> {item.Project.ShortProjectName}[/]");
            }
        }

        AnsiConsole.MarkupLine(new string('-', 60));
        AnsiConsole.MarkupLine($"[bold]Total Work Time:[/]    {(int)totalWork.TotalHours}h {totalWork.Minutes}m");
        AnsiConsole.MarkupLine($"[bold]Total Break Time:[/]   {(int)totalBreak.TotalHours}h {totalBreak.Minutes}m");

        return 0;
    }

    public void PrintHelp()
    {
        AnsiConsole.MarkupLine("Usage: time list --user <username> [--date <yyyy-MM-dd>]");
        AnsiConsole.MarkupLine("Lists all time items for a user on a specific date.");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("Options:");
        AnsiConsole.MarkupLine("  --user, -u    Username (required)");
        AnsiConsole.MarkupLine("  --date, -d    Date in yyyy-MM-dd format (default: today)");
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Legatro.DataLayer.Context;
using Legatro.DataLayer.DemoData;
using Legatro.DataLayer.Extensions;
using LegatroTestConsole;
using LegatroTestConsole.Commands;

// Build host
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Database path
        var dbPath = Path.Combine(Environment.CurrentDirectory, "legatro.db");

        // Add Legatro data layer
        services.AddLegatroDataLayer(DatabaseProviderType.Sqlite, $"Data Source={dbPath}");

        // Register demo database generator
        services.AddSingleton<IDemoDatabaseGenerator>(sp =>
            new LegatroDemoDatabaseGenerator(dbPath));

        // Register console application
        services.AddSingleton<ConsoleApplication>();

        // Register command handlers
        services.AddTransient<ICommandHandler, UserListCommand>();
        services.AddTransient<ICommandHandler, ProjectListCommand>();
        services.AddTransient<ICommandHandler, TimeListCommand>();
        services.AddTransient<ICommandHandler, DbRegenerateCommand>();
        services.AddTransient<ICommandHandler, HelpCommand>();
    })
    .Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LegatroDbContext>();
    await context.Database.EnsureCreatedAsync();
}

// Run console application
var app = host.Services.GetRequiredService<ConsoleApplication>();
await app.RunAsync();

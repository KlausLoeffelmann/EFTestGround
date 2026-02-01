using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Legatro.DataLayer.Context;

namespace Legatro.Tests.DataLayer.Fixtures;

/// <summary>
/// Factory for creating in-memory SQLite database contexts for testing.
/// </summary>
public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Creates a new in-memory SQLite database context.
    /// FK constraints are disabled to allow testing business logic without seeding all reference data.
    /// </summary>
    public static LegatroDbContext Create()
    {
        var connectionString = $"Data Source=:memory:";
        var options = new DbContextOptionsBuilder<LegatroDbContext>()
            .UseSqlite(connectionString)
            .Options;

        var context = new LegatroDbContext(options);

        // Open connection and create schema
        context.Database.OpenConnection();

        // Disable FK constraints for testing - allows testing business logic without seeding all reference data
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a new file-based SQLite database context.
    /// </summary>
    public static LegatroDbContext CreateWithFile(string path)
    {
        var connectionString = $"Data Source={path}";
        var options = new DbContextOptionsBuilder<LegatroDbContext>()
            .UseSqlite(connectionString)
            .Options;

        var context = new LegatroDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}

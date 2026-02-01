using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Legatro.DataLayer.Context;

/// <summary>
/// Factory for creating LegatroDbContext instances with different database providers.
/// Also implements IDesignTimeDbContextFactory for EF Core migrations.
/// </summary>
public class LegatroDbContextFactory : IDesignTimeDbContextFactory<LegatroDbContext>
{
    /// <summary>
    /// Creates a new LegatroDbContext for the specified database provider.
    /// </summary>
    /// <param name="provider">The database provider to use.</param>
    /// <param name="connectionString">The connection string for the database.</param>
    /// <returns>A configured LegatroDbContext instance.</returns>
    public static LegatroDbContext Create(DatabaseProviderType provider, string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LegatroDbContext>();
        ConfigureOptions(optionsBuilder, provider, connectionString);
        return new LegatroDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Creates an options builder configured for the specified provider.
    /// </summary>
    public static DbContextOptionsBuilder<LegatroDbContext> CreateOptionsBuilder(
        DatabaseProviderType provider,
        string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LegatroDbContext>();
        ConfigureOptions(optionsBuilder, provider, connectionString);
        return optionsBuilder;
    }

    /// <summary>
    /// Configures the DbContextOptionsBuilder for the specified provider.
    /// </summary>
    public static void ConfigureOptions(
        DbContextOptionsBuilder optionsBuilder,
        DatabaseProviderType provider,
        string connectionString)
    {
        switch (provider)
        {
            case DatabaseProviderType.Sqlite:
                optionsBuilder.UseSqlite(connectionString);
                break;

            case DatabaseProviderType.SqlServerLocalDb:
            case DatabaseProviderType.SqlServer:
                optionsBuilder.UseSqlServer(connectionString, options =>
                {
                    options.UseNetTopologySuite();
                });
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider,
                    "Unsupported database provider type.");
        }
    }

    /// <summary>
    /// Creates a context for design-time operations (EF Core migrations).
    /// Uses SQLite by default for design-time.
    /// </summary>
    public LegatroDbContext CreateDbContext(string[] args)
    {
        // Default to SQLite for design-time operations
        return Create(DatabaseProviderType.Sqlite, "Data Source=legatro-design.db");
    }
}

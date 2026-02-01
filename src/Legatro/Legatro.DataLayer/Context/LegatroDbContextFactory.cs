using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Legatro.DataLayer.Context;

/// <summary>
/// Factory for creating DbContext instances with support for multiple database providers.
/// </summary>
public class LegatroDbContextFactory : IDesignTimeDbContextFactory<LegatroDbContext>, IDbContextFactory<LegatroDbContext>
{
    private readonly DatabaseProvider _provider;
    private readonly string _connectionString;
    private readonly bool _enableSensitiveDataLogging;

    /// <summary>
    /// Creates a factory with the specified provider and connection string.
    /// </summary>
    public LegatroDbContextFactory(DatabaseProvider provider, string connectionString, bool enableSensitiveDataLogging = true)
    {
        _provider = provider;
        _connectionString = connectionString;
        _enableSensitiveDataLogging = enableSensitiveDataLogging;
    }

    /// <summary>
    /// Creates a new DbContext instance (for design-time use).
    /// </summary>
    public LegatroDbContext CreateDbContext()
    {
        return CreateDbContext(null);
    }

    /// <summary>
    /// Creates a new DbContext instance with the specified arguments.
    /// </summary>
    public LegatroDbContext CreateDbContext(string[]? args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LegatroDbContext>();

        ConfigureOptions(optionsBuilder);

        return new LegatroDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Configures the DbContext options based on the provider.
    /// </summary>
    private void ConfigureOptions(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable sensitive data logging in development
        if (_enableSensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        switch (_provider)
        {
            case DatabaseProvider.Sqlite:
                optionsBuilder.UseSqlite(_connectionString, options =>
                {
                    options.UseNetTopologySuite();
                });
                break;

            case DatabaseProvider.SqlServerLocalDb:
            case DatabaseProvider.SqlServer:
                optionsBuilder.UseSqlServer(_connectionString, options =>
                {
                    options.UseNetTopologySuite();
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
                break;

            default:
                throw new ArgumentException($"Unsupported database provider: {_provider}");
        }
    }

    /// <summary>
    /// Creates a factory for SQLite with the specified connection string.
    /// </summary>
    public static LegatroDbContextFactory CreateForSqlite(string connectionString, bool enableSensitiveDataLogging = true)
    {
        return new LegatroDbContextFactory(DatabaseProvider.Sqlite, connectionString, enableSensitiveDataLogging);
    }

    /// <summary>
    /// Creates a factory for SQL Server LocalDB with the specified connection string.
    /// </summary>
    public static LegatroDbContextFactory CreateForSqlServerLocalDb(string connectionString, bool enableSensitiveDataLogging = true)
    {
        return new LegatroDbContextFactory(DatabaseProvider.SqlServerLocalDb, connectionString, enableSensitiveDataLogging);
    }

    /// <summary>
    /// Creates a factory for SQL Server with the specified connection string.
    /// </summary>
    public static LegatroDbContextFactory CreateForSqlServer(string connectionString, bool enableSensitiveDataLogging = true)
    {
        return new LegatroDbContextFactory(DatabaseProvider.SqlServer, connectionString, enableSensitiveDataLogging);
    }
}
using Legatro.DataLayer.Context;
using Legatro.DataLayer.Seeding;
using Legatro.DataLayer.Services;
using Legatro.DataLayer.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Legatro.DataLayer.Extensions;

/// <summary>
/// Extension methods for configuring Legatro data layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Legatro data layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="provider">The database provider to use.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="enableSensitiveDataLogging">Whether to enable sensitive data logging (development only).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLegatroDataLayer(
        this IServiceCollection services,
        DatabaseProvider provider,
        string connectionString,
        bool enableSensitiveDataLogging = true)
    {
        // Register the DbContext factory
        var factory = new LegatroDbContextFactory(provider, connectionString, enableSensitiveDataLogging);
        services.AddSingleton(factory);

        // Register the validator as scoped (per request)
        services.AddScoped<TimeItemValidator>();

        // Register the database seeder as scoped
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

        // Register the TimeItem service as scoped
        services.AddScoped<ITimeItemService, TimeItemService>();

        return services;
    }

    /// <summary>
    /// Adds the Legatro data layer with SQLite provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <param name="enableSensitiveDataLogging">Whether to enable sensitive data logging.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLegatroDataLayerWithSqlite(
        this IServiceCollection services,
        string connectionString,
        bool enableSensitiveDataLogging = true)
    {
        return services.AddLegatroDataLayer(
            DatabaseProvider.Sqlite,
            connectionString,
            enableSensitiveDataLogging);
    }

    /// <summary>
    /// Adds the Legatro data layer with SQL Server provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="enableSensitiveDataLogging">Whether to enable sensitive data logging.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLegatroDataLayerWithSqlServer(
        this IServiceCollection services,
        string connectionString,
        bool enableSensitiveDataLogging = true)
    {
        return services.AddLegatroDataLayer(
            DatabaseProvider.SqlServer,
            connectionString,
            enableSensitiveDataLogging);
    }
}
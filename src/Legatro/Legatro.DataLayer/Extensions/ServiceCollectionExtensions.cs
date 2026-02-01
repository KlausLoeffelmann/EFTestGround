using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Legatro.DataLayer.Context;
using Legatro.DataLayer.Services.Interfaces;
using Legatro.DataLayer.Services.TimeItem;
using Legatro.DataLayer.Validators;

namespace Legatro.DataLayer.Extensions;

/// <summary>
/// Extension methods for registering Legatro data layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Legatro data layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="provider">The database provider type.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLegatroDataLayer(
        this IServiceCollection services,
        DatabaseProviderType provider,
        string connectionString)
    {
        services.AddDbContext<LegatroDbContext>(options =>
        {
            LegatroDbContextFactory.ConfigureOptions(options, provider, connectionString);
        });

        // Register TimeItem services
        services.AddScoped<ITimeItemValidator, TimeItemValidator>();
        services.AddScoped<ITimeItemLinkedListManager, TimeItemLinkedListManager>();
        services.AddScoped<ITimeItemVersioningService, TimeItemVersioningService>();
        services.AddScoped<ITimeItemService, TimeItemService>();

        return services;
    }

    /// <summary>
    /// Adds the Legatro data layer services with a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for data layer options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLegatroDataLayer(
        this IServiceCollection services,
        Action<LegatroDataLayerOptions> configure)
    {
        var options = new LegatroDataLayerOptions();
        configure(options);

        return services.AddLegatroDataLayer(options.Provider, options.ConnectionString);
    }
}

/// <summary>
/// Options for configuring the Legatro data layer.
/// </summary>
public class LegatroDataLayerOptions
{
    /// <summary>
    /// The database provider type to use.
    /// </summary>
    public DatabaseProviderType Provider { get; set; } = DatabaseProviderType.Sqlite;

    /// <summary>
    /// The database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=legatro.db";
}

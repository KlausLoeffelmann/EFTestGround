namespace Legatro.DataLayer.Seeding;

/// <summary>
/// Interface for database seeding operations.
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>
    /// Seeds all default data into the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SeedAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds default TimeItemTypes into the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SeedTimeItemTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds a system user into the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SeedSystemUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds default categories into the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SeedDefaultCategoriesAsync(CancellationToken cancellationToken = default);
}
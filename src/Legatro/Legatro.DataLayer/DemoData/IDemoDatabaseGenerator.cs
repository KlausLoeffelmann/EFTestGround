namespace Legatro.DataLayer.DemoData;

/// <summary>
/// Interface for generating demo database with realistic test data.
/// </summary>
public interface IDemoDatabaseGenerator
{
    /// <summary>
    /// Generates a complete demo database with the specified configuration.
    /// </summary>
    /// <param name="config">Configuration for data generation.</param>
    /// <param name="ct">Cancellation token.</param>
    Task GenerateAsync(DemoDataConfiguration config, CancellationToken ct = default);

    /// <summary>
    /// Clears all data and regenerates the database.
    /// </summary>
    /// <param name="config">Configuration for data generation.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RegenerateAsync(DemoDataConfiguration config, CancellationToken ct = default);

    /// <summary>
    /// Returns the path to the generated SQLite database file.
    /// </summary>
    string GetDatabasePath();
}

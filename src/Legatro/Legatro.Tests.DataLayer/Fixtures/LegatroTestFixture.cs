using Legatro.DataLayer.Context;
using Legatro.Tests.DataLayer.Models;
using Microsoft.Extensions.Hosting;

namespace Legatro.Tests.DataLayer.Fixtures;

/// <summary>
/// Test fixture that provides a fresh database for each test.
/// </summary>
public class LegatroTestFixture : IAsyncLifetime
{
    private LegatroDbContextFactory? _factory;
    private LegatroDemoDatabaseGenerator? _generator;
    private string? _databasePath;

    /// <summary>
    /// Gets the DbContextFactory for creating contexts.
    /// </summary>
    public LegatroDbContextFactory Factory => _factory ?? throw new InvalidOperationException("Factory not initialized");

    /// <summary>
    /// Gets the demo database generator.
    /// </summary>
    public LegatroDemoDatabaseGenerator Generator => _generator ?? throw new InvalidOperationException("Generator not initialized");

    /// <summary>
    /// Gets a fresh DbContext instance.
    /// </summary>
    public LegatroDbContext CreateContext() => Factory.CreateDbContext();

    /// <summary>
    /// Initializes the test fixture before any tests run.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        _databasePath = GetUniqueDatabasePath();
        _factory = LegatroDbContextFactory.CreateForSqlite(
            $"Data Source={_databasePath}",
            enableSensitiveDataLogging: true);

        _generator = new LegatroDemoDatabaseGenerator(new DemoDataConfiguration(), _factory);
        await _generator.GenerateAsync();
    }

    /// <summary>
    /// Cleans up the test fixture after all tests run.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_databasePath != null && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    /// <summary>
    /// Generates a unique database path for this test session.
    /// </summary>
    private string GetUniqueDatabasePath()
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "LegatroTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempFolder);

        return Path.Combine(tempFolder, "test.db");
    }
}

/// <summary>
/// Test fixture collection that ensures the database is only initialized once per test run.
/// </summary>
[CollectionDefinition("Legatro Tests")]
public class LegatroTestCollection : ICollectionFixture<LegatroTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place where [CollectionDefinition] attributes attach so that
    // the fixture can be shared across test classes.
}
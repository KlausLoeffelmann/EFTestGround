using Legatro.DataLayer.Context;
using Legatro.DataLayer.DemoData;

namespace Legatro.Tests.DataLayer.Fixtures;

/// <summary>
/// Test fixture that provides a pre-populated database for integration tests.
/// </summary>
public class LegatroTestFixture : IAsyncLifetime
{
    private LegatroDbContext? _context;
    private readonly string _databasePath;

    public LegatroDbContext Context => _context ?? throw new InvalidOperationException("Fixture not initialized");
    public IDemoDatabaseGenerator Generator { get; }

    public LegatroTestFixture()
    {
        _databasePath = $"legatro-test-{Guid.NewGuid()}.db";
        Generator = new LegatroDemoDatabaseGenerator(_databasePath);
    }

    public async ValueTask InitializeAsync()
    {
        await Generator.RegenerateAsync(new DemoDataConfiguration
        {
            TimeDataWeeks = 2,
            RandomSeed = 12345
        });

        _context = InMemoryDbContextFactory.CreateWithFile(_databasePath);
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        // Clean up database file
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}

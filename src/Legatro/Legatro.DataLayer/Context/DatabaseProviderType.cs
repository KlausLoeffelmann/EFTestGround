namespace Legatro.DataLayer.Context;

/// <summary>
/// Defines the available database providers for the Legatro data layer.
/// </summary>
public enum DatabaseProviderType
{
    /// <summary>
    /// SQLite database - for local development, testing, and lightweight deployments.
    /// </summary>
    Sqlite,

    /// <summary>
    /// SQL Server LocalDB - for Windows development environments.
    /// </summary>
    SqlServerLocalDb,

    /// <summary>
    /// SQL Server - for production deployments.
    /// </summary>
    SqlServer
}

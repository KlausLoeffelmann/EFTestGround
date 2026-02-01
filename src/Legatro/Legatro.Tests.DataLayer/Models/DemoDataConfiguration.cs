namespace Legatro.Tests.DataLayer.Models;

/// <summary>
/// Configuration for generating demo database data.
/// </summary>
public class DemoDataConfiguration
{
    /// <summary>
    /// Number of users to generate.
    /// </summary>
    public int NumberOfUsers { get; set; } = 3;

    /// <summary>
    /// Number of projects per user to generate.
    /// </summary>
    public int ProjectsPerUser { get; set; } = 3;

    /// <summary>
    /// Number of tasks per project to generate.
    /// </summary>
    public int TasksPerProject { get; set; } = 5;

    /// <summary>
    /// Number of customers to generate.
    /// </summary>
    public int NumberOfCustomers { get; set; } = 5;

    /// <summary>
    /// Number of weeks of time data to generate.
    /// </summary>
    public int TimeDataWeeks { get; set; } = 4;

    /// <summary>
    /// Start date for time data generation.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-28);
}
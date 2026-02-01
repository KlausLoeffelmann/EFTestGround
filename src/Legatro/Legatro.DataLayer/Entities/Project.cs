namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a project that can contain tasks and time tracking targets.
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdProject { get; set; }

    /// <summary>
    /// User who owns this project.
    /// </summary>
    public Guid? IdUserAsOwner { get; set; }

    /// <summary>
    /// Navigation property to the project owner.
    /// </summary>
    public User? Owner { get; set; }

    /// <summary>
    /// Parent project for hierarchical project structure.
    /// </summary>
    public Guid? IdParentProject { get; set; }

    /// <summary>
    /// Navigation property to the parent project.
    /// </summary>
    public Project? ParentProject { get; set; }

    /// <summary>
    /// Navigation property to child projects.
    /// </summary>
    public virtual ICollection<Project> ChildProjects { get; set; } = new List<Project>();

    /// <summary>
    /// Customer associated with this project.
    /// </summary>
    public Guid? IdCustomer { get; set; }

    /// <summary>
    /// Navigation property to the customer.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// External reference ID for integration with external systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Project number.
    /// </summary>
    public int ProjectNumber { get; set; }

    /// <summary>
    /// Name of the project.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Short name for the project.
    /// </summary>
    public string ShortProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is a project.
    /// </summary>
    public bool IsProject { get; set; }

    /// <summary>
    /// Indicates whether this project is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates whether this is a sub-project.
    /// </summary>
    public bool IsSubProject { get; set; }

    /// <summary>
    /// Description of the project.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether time capacity should be monitored.
    /// </summary>
    public bool? MonitorTimeCapacity { get; set; }

    /// <summary>
    /// Monthly target time capacity in minutes.
    /// </summary>
    public int? MonthlyTargetTimeCapacity { get; set; }

    /// <summary>
    /// Start date for capacity monitoring.
    /// </summary>
    public DateTime? MonitorStartdate { get; set; }

    /// <summary>
    /// End date for capacity monitoring.
    /// </summary>
    public DateTime? MonitorEnddate { get; set; }

    /// <summary>
    /// Total target time capacity.
    /// </summary>
    public int? TotalTargetTimeCapacity { get; set; }

    /// <summary>
    /// Indicates whether production units should be collected.
    /// </summary>
    public bool CollectProductionUnits { get; set; }

    /// <summary>
    /// Indicates whether production unit descriptions should be collected.
    /// </summary>
    public bool CollectProductionUnitsDescription { get; set; }

    /// <summary>
    /// Indicates whether this project has associated customers.
    /// </summary>
    public bool HasCustomers { get; set; }

    /// <summary>
    /// Maximum number of employees that can be booked to this project.
    /// </summary>
    public int? MaxBookedEmployees { get; set; }

    // Navigation properties (reverse)
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
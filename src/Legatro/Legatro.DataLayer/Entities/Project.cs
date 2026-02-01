using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a project container for tasks and time tracking.
/// Supports hierarchical structure with parent/child relationships.
/// </summary>
public class Project : BaseEntity, ISoftDeletable
{
    public Guid IdProject { get; set; }

    /// <summary>
    /// Reference to the user who owns this project.
    /// </summary>
    public Guid? IdUserAsOwner { get; set; }

    /// <summary>
    /// Reference to parent project for sub-project hierarchy.
    /// </summary>
    public Guid? IdParentProject { get; set; }

    /// <summary>
    /// Reference to the customer this project is for.
    /// </summary>
    public Guid? IdCustomer { get; set; }

    /// <summary>
    /// External reference ID for integration with other systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    public int ProjectNumber { get; set; }
    public string ProjectName { get; set; } = null!;
    public string ShortProjectName { get; set; } = null!;

    public bool IsProject { get; set; }
    public bool IsActive { get; set; }
    public bool IsSubProject { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// If true, enables budget tracking for this project.
    /// </summary>
    public bool? MonitorTimeCapacity { get; set; }

    /// <summary>
    /// Monthly target capacity in minutes.
    /// </summary>
    public int? MonthlyTargetTimeCapacity { get; set; }

    public DateTime? MonitorStartdate { get; set; }
    public DateTime? MonitorEnddate { get; set; }

    /// <summary>
    /// Total target capacity in minutes for the project lifetime.
    /// </summary>
    public int? TotalTargetTimeCapacity { get; set; }

    public bool CollectProductionUnits { get; set; }
    public bool CollectProductionUnitsDescription { get; set; }

    public bool HasCustomers { get; set; }

    /// <summary>
    /// Maximum number of employees that can work on this project concurrently.
    /// </summary>
    public int? MaxBookedEmployees { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }

    // Navigation properties
    public virtual User? Owner { get; set; }
    public virtual Project? ParentProject { get; set; }
    public virtual ICollection<Project> SubProjects { get; set; } = new List<Project>();
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<LegatroTask> Tasks { get; set; } = new List<LegatroTask>();
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
}

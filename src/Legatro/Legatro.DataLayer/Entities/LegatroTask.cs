using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a work item that can be tracked against time.
/// Supports hierarchical structure with parent/child relationships.
/// Named LegatroTask to avoid conflict with System.Threading.Tasks.Task.
/// </summary>
public class LegatroTask : BaseEntity, ISoftDeletable
{
    public Guid IdTask { get; set; }

    /// <summary>
    /// Reference to parent task for hierarchical task structure.
    /// </summary>
    public Guid? IdParentTask { get; set; }

    /// <summary>
    /// Reference to the user who created this task.
    /// </summary>
    public Guid? IdUserTaskCreated { get; set; }

    /// <summary>
    /// Reference to the user who owns/is assigned to this task.
    /// </summary>
    public Guid? IdUserAsOwner { get; set; }

    /// <summary>
    /// Reference to the project this task belongs to.
    /// </summary>
    public Guid? IdProject { get; set; }

    /// <summary>
    /// External reference ID for integration (MS To-Do, AzDO, Git, etc.).
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    public string TaskName { get; set; } = null!;
    public string TaskDescription { get; set; } = null!;

    public int? TaskNo { get; set; }
    public int TaskOrderNo { get; set; }

    public DateTime? DueDate { get; set; }

    public bool TaskDone { get; set; }
    public DateTime? TaskDoneDate { get; set; }

    /// <summary>
    /// Total time spent on this task in minutes.
    /// </summary>
    public int? TimeInMinutesWorkedOn { get; set; }

    /// <summary>
    /// Planned capacity for this task in minutes.
    /// </summary>
    public int? PlanedCapacityInMinutes { get; set; }

    /// <summary>
    /// Minutes before due date to send a reminder.
    /// </summary>
    public int? RememberMinutesBefore { get; set; }

    /// <summary>
    /// If true, this task is a template.
    /// </summary>
    public bool IsTemplate { get; set; }

    public double? TimePerUnit { get; set; }
    public double? DegreeOfDifficultyProMille { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }

    // Navigation properties
    public virtual LegatroTask? ParentTask { get; set; }
    public virtual ICollection<LegatroTask> SubTasks { get; set; } = new List<LegatroTask>();
    public virtual Project? Project { get; set; }
    public virtual User? Owner { get; set; }
    public virtual User? Creator { get; set; }
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
}

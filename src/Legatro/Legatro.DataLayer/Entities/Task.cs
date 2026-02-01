namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a task that can be tracked against time.
/// </summary>
public class Task : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdTask { get; set; }

    /// <summary>
    /// Parent task for hierarchical task structure.
    /// </summary>
    public Guid? IdParentTask { get; set; }

    /// <summary>
    /// Navigation property to the parent task.
    /// </summary>
    public Task? ParentTask { get; set; }

    /// <summary>
    /// Navigation property to child tasks.
    /// </summary>
    public virtual ICollection<Task> ChildTasks { get; set; } = new List<Task>();

    /// <summary>
    /// User who created this task.
    /// </summary>
    public Guid? IdUserTaskCreated { get; set; }

    /// <summary>
    /// Navigation property to the user who created this task.
    /// </summary>
    public User? TaskCreatedBy { get; set; }

    /// <summary>
    /// User who owns/is assigned to this task.
    /// </summary>
    public Guid? IdUserAsOwner { get; set; }

    /// <summary>
    /// Navigation property to the task owner.
    /// </summary>
    public User? Owner { get; set; }

    /// <summary>
    /// Project this task belongs to.
    /// </summary>
    public Guid? IdProject { get; set; }

    /// <summary>
    /// Navigation property to the project.
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// External reference ID for integration with external systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Name of the task.
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the task.
    /// </summary>
    public string TaskDescription { get; set; } = string.Empty;

    /// <summary>
    /// Task number.
    /// </summary>
    public int? TaskNo { get; set; }

    /// <summary>
    /// Task order number for sorting.
    /// </summary>
    public int TaskOrderNo { get; set; }

    /// <summary>
    /// Due date for the task.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Indicates whether the task is completed.
    /// </summary>
    public bool TaskDone { get; set; }

    /// <summary>
    /// Date when the task was marked as done.
    /// </summary>
    public DateTime? TaskDoneDate { get; set; }

    /// <summary>
    /// Total time in minutes worked on this task.
    /// </summary>
    public int? TimeInMinutesWorkedOn { get; set; }

    /// <summary>
    /// Planned capacity in minutes for this task.
    /// </summary>
    public int? PlanedCapacityInMinutes { get; set; }

    /// <summary>
    /// Number of minutes before due date to send a reminder.
    /// </summary>
    public int? RememberMinutesBefore { get; set; }

    /// <summary>
    /// Indicates whether this is a template task.
    /// </summary>
    public bool IsTemplate { get; set; }

    /// <summary>
    /// Time per unit for calculation purposes.
    /// </summary>
    public double? TimePerUnit { get; set; }

    /// <summary>
    /// Degree of difficulty (in promille).
    /// </summary>
    public double? DegreeOfDifficultyProMille { get; set; }

    // Navigation properties (reverse)
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
}
using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents an employee/user identity in the time tracking system.
/// </summary>
public class User : BaseEntity, ISoftDeletable
{
    public Guid IdUser { get; set; }

    /// <summary>
    /// Reference to the user this shadow employee can book for.
    /// </summary>
    public Guid? IdUserForShadow { get; set; }

    /// <summary>
    /// Reference to the user's contact information.
    /// </summary>
    public Guid IdContact { get; set; }

    /// <summary>
    /// External reference ID for integration with other systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Shadow employees can book on behalf of the linked real user.
    /// </summary>
    public bool IsShadowEmployee { get; set; }

    /// <summary>
    /// If true, automatically book for shadow user implicitly.
    /// </summary>
    public bool AutoBookForShadowImplicitly { get; set; }

    public string? PersonnelNumber { get; set; }
    public string? Matchcode { get; set; }

    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string Username { get; set; } = null!;

    public byte[]? Password { get; set; }

    /// <summary>
    /// Security clearance level for access control.
    /// </summary>
    public long ClearanceLevel { get; set; }

    public DateTime? DateLastTriggered { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActivated { get; set; }
    public DateTime? ExpireDate { get; set; }
    public bool IsSystemAccount { get; set; }

    public DateTime? DateOfJoining { get; set; }
    public DateTime? DateOfSeparation { get; set; }

    public string? RfId { get; set; }
    public string? TimeCardNo { get; set; }
    public string? Comment { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }

    // Navigation properties
    public virtual Contact Contact { get; set; } = null!;
    public virtual User? ShadowTarget { get; set; }
    public virtual ICollection<User> ShadowUsers { get; set; } = new List<User>();
    public virtual ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public virtual ICollection<LegatroTask> OwnedTasks { get; set; } = new List<LegatroTask>();
    public virtual ICollection<LegatroTask> CreatedTasks { get; set; } = new List<LegatroTask>();
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
}

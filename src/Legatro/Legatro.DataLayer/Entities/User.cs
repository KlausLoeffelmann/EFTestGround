namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a user/employee in the system.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdUser { get; set; }

    /// <summary>
    /// If this is a shadow employee, links to the real user being shadowed.
    /// </summary>
    public Guid? IdUserForShadow { get; set; }

    /// <summary>
    /// Navigation property to the shadow user.
    /// </summary>
    public User? ShadowUser { get; set; }

    /// <summary>
    /// Navigation property to users shadowing this user.
    /// </summary>
    public virtual ICollection<User> ShadowingUsers { get; set; } = new List<User>();

    /// <summary>
    /// Contact information for this user.
    /// </summary>
    public Guid IdContact { get; set; }

    /// <summary>
    /// Navigation property to the contact.
    /// </summary>
    public Contact Contact { get; set; } = null!;

    /// <summary>
    /// External reference ID for integration with external systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Indicates whether this is a shadow employee account.
    /// </summary>
    public bool IsShadowEmployee { get; set; }

    /// <summary>
    /// Indicates whether time should be automatically booked for the shadow user.
    /// </summary>
    public bool AutoBookForShadowImplicitly { get; set; }

    /// <summary>
    /// Personnel number.
    /// </summary>
    public string? PersonnelNumber { get; set; }

    /// <summary>
    /// Matchcode for quick lookup.
    /// </summary>
    public string? Matchcode { get; set; }

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Middle name.
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Username for login.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password hash.
    /// </summary>
    public byte[]? Password { get; set; }

    /// <summary>
    /// Clearance level for access control.
    /// </summary>
    public long ClearanceLevel { get; set; }

    /// <summary>
    /// Last date when this user was triggered/activated.
    /// </summary>
    public DateTime? DateLastTriggered { get; set; }

    /// <summary>
    /// Indicates whether this user has admin privileges.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Indicates whether this user is active.
    /// </summary>
    public bool IsActivated { get; set; }

    /// <summary>
    /// Expiration date for this user account.
    /// </summary>
    public DateTime? ExpireDate { get; set; }

    /// <summary>
    /// Indicates whether this is a system account.
    /// </summary>
    public bool IsSystemAccount { get; set; }

    /// <summary>
    /// Date of joining.
    /// </summary>
    public DateTime? DateOfJoining { get; set; }

    /// <summary>
    /// Date of separation/termination.
    /// </summary>
    public DateTime? DateOfSeparation { get; set; }

    /// <summary>
    /// RFID card number.
    /// </summary>
    public string? RfId { get; set; }

    /// <summary>
    /// Time card number.
    /// </summary>
    public string? TimeCardNo { get; set; }

    /// <summary>
    /// Comment/notes.
    /// </summary>
    public string? Comment { get; set; }

    // Navigation properties (reverse)
    public virtual ICollection<TimeItem> TimeItems { get; set; } = new List<TimeItem>();
    public virtual ICollection<Task> OwnedTasks { get; set; } = new List<Task>();
    public virtual ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
}
namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents an external client for project billing and association.
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdCustomer { get; set; }

    /// <summary>
    /// Company contact information.
    /// </summary>
    public Guid IdCompanyContact { get; set; }

    /// <summary>
    /// Navigation property to the company contact.
    /// </summary>
    public Contact CompanyContact { get; set; } = null!;

    /// <summary>
    /// Main contact person.
    /// </summary>
    public Guid? IdMainContact { get; set; }

    /// <summary>
    /// Navigation property to the main contact.
    /// </summary>
    public Contact? MainContact { get; set; }

    /// <summary>
    /// Matchcode for quick lookup.
    /// </summary>
    public string? Matchcode { get; set; }

    /// <summary>
    /// External reference ID for integration with external systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Customer number.
    /// </summary>
    public int CustomerNumber { get; set; }

    /// <summary>
    /// Name of the customer.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is an individual (B2C) vs company (B2B).
    /// </summary>
    public bool IsIndividual { get; set; }

    /// <summary>
    /// Indicates whether this customer is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Comment/notes.
    /// </summary>
    public string? Comment { get; set; }

    // Navigation properties (reverse)
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
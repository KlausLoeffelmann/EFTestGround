using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents an external client for project billing and association.
/// </summary>
public class Customer : BaseEntity, ISoftDeletable
{
    public Guid IdCustomer { get; set; }

    /// <summary>
    /// Reference to the customer's company contact information.
    /// </summary>
    public Guid IdCompanyContact { get; set; }

    /// <summary>
    /// Reference to the main contact person.
    /// </summary>
    public Guid? IdMainContact { get; set; }

    public string? Matchcode { get; set; }

    /// <summary>
    /// External reference ID for integration with other systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    public int CustomerNumber { get; set; }
    public string CompanyName { get; set; } = null!;

    /// <summary>
    /// Distinguishes B2C (individual) from B2B (company) customers.
    /// </summary>
    public bool IsIndividual { get; set; }

    /// <summary>
    /// Only active customers appear in selections.
    /// </summary>
    public bool IsActive { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }

    // Navigation properties
    public virtual Contact CompanyContact { get; set; } = null!;
    public virtual Contact? MainContact { get; set; }
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}

using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a supplier or external service provider.
/// </summary>
public class Vendor : BaseEntity, ISoftDeletable
{
    public Guid IdVendor { get; set; }

    /// <summary>
    /// Reference to the vendor's company contact information.
    /// </summary>
    public Guid IdVendorContact { get; set; }

    /// <summary>
    /// Reference to the main contact person.
    /// </summary>
    public Guid? IdMainContact { get; set; }

    public string? Matchcode { get; set; }

    /// <summary>
    /// External reference ID for integration with other systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    public int VendorNumber { get; set; }
    public string CompanyName { get; set; } = null!;

    /// <summary>
    /// Indicates if this vendor is an individual rather than a company.
    /// </summary>
    public bool IsIndividual { get; set; }

    /// <summary>
    /// Only active vendors appear in selections.
    /// </summary>
    public bool IsActive { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }

    // Navigation properties
    public virtual Contact VendorContact { get; set; } = null!;
    public virtual Contact? MainContact { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

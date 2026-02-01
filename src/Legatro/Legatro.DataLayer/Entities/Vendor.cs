namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents a supplier or external service provider.
/// </summary>
public class Vendor : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdVendor { get; set; }

    /// <summary>
    /// Vendor contact information.
    /// </summary>
    public Guid IdVendorContact { get; set; }

    /// <summary>
    /// Navigation property to the vendor contact.
    /// </summary>
    public Contact VendorContact { get; set; } = null!;

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
    /// Vendor number.
    /// </summary>
    public int VendorNumber { get; set; }

    /// <summary>
    /// Name of the vendor.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is an individual vendor.
    /// </summary>
    public bool IsIndividual { get; set; }

    /// <summary>
    /// Indicates whether this vendor is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Comment/notes.
    /// </summary>
    public string? Comment { get; set; }

    // Navigation properties (reverse)
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
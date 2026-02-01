using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents contact information shared by User, Customer, and Vendor entities.
/// Supports hierarchical structure via self-reference.
/// </summary>
public class Contact : BaseEntity, ISoftDeletable
{
    public Guid IdContact { get; set; }

    /// <summary>
    /// Reference to parent contact for organizational hierarchy.
    /// </summary>
    public Guid? IdParentContact { get; set; }

    /// <summary>
    /// External reference ID for integration with other systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    public string? Salutation { get; set; }
    public string MainName { get; set; } = null!;
    public string? AdditionalName1 { get; set; }
    public string? AdditionalName2 { get; set; }

    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }

    public string? Zip { get; set; }
    public string? POBox { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public string? Email { get; set; }
    public string? PhoneBusiness { get; set; }
    public string? PhonePrivate { get; set; }
    public string? PhoneMobile { get; set; }

    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? IsDeleted { get; set; }

    // Navigation properties
    public virtual Contact? ParentContact { get; set; }
    public virtual ICollection<Contact> ChildContacts { get; set; } = new List<Contact>();
}

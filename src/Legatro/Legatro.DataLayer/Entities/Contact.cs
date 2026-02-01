using NetTopologySuite.Geometries;

namespace Legatro.DataLayer.Entities;

/// <summary>
/// Represents contact information for users, customers, and vendors.
/// </summary>
public class Contact : BaseEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public Guid IdContact { get; set; }

    /// <summary>
    /// Self-referencing parent contact for organizational hierarchies.
    /// </summary>
    public Guid? IdParentContact { get; set; }

    /// <summary>
    /// Navigation property to the parent contact.
    /// </summary>
    public Contact? ParentContact { get; set; }

    /// <summary>
    /// Navigation property to child contacts.
    /// </summary>
    public virtual ICollection<Contact> ChildContacts { get; set; } = new List<Contact>();

    /// <summary>
    /// External reference ID for integration with external systems.
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Salutation (Mr., Ms., Dr., etc.).
    /// </summary>
    public string? Salutation { get; set; }

    /// <summary>
    /// Main name of the contact.
    /// </summary>
    public string MainName { get; set; } = string.Empty;

    /// <summary>
    /// Additional name field 1.
    /// </summary>
    public string? AdditionalName1 { get; set; }

    /// <summary>
    /// Additional name field 2.
    /// </summary>
    public string? AdditionalName2 { get; set; }

    /// <summary>
    /// Address line 1.
    /// </summary>
    public string? Address1 { get; set; }

    /// <summary>
    /// Address line 2.
    /// </summary>
    public string? Address2 { get; set; }

    /// <summary>
    /// Address line 3.
    /// </summary>
    public string? Address3 { get; set; }

    /// <summary>
    /// ZIP/Postal code.
    /// </summary>
    public string? Zip { get; set; }

    /// <summary>
    /// PO Box.
    /// </summary>
    public string? POBox { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Country.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Business phone number.
    /// </summary>
    public string? PhoneBusiness { get; set; }

    /// <summary>
    /// Private phone number.
    /// </summary>
    public string? PhonePrivate { get; set; }

    /// <summary>
    /// Mobile phone number.
    /// </summary>
    public string? PhoneMobile { get; set; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    // Navigation properties (reverse)
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Customer> CustomersAsCompany { get; set; } = new List<Customer>();
    public virtual ICollection<Customer> CustomersAsMain { get; set; } = new List<Customer>();
    public virtual ICollection<Vendor> VendorsAsCompany { get; set; } = new List<Vendor>();
    public virtual ICollection<Vendor> VendorsAsMain { get; set; } = new List<Vendor>();
}
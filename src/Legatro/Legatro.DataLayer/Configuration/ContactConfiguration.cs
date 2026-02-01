using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the Contact entity.
/// </summary>
public class ContactConfiguration : BaseEntityConfiguration<Contact>
{
    public override void Configure(EntityTypeBuilder<Contact> builder)
    {
        base.Configure(builder);

        builder.ToTable("Contacts");

        // Primary key
        builder.HasKey(e => e.IdContact);

        // Configure IdContact with default value
        builder.Property(e => e.IdContact)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.ExternalReferenceId)
            .HasMaxLength(128);

        builder.Property(e => e.Salutation)
            .HasMaxLength(100);

        builder.Property(e => e.MainName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.AdditionalName1)
            .HasMaxLength(100);

        builder.Property(e => e.AdditionalName2)
            .HasMaxLength(100);

        builder.Property(e => e.Address1)
            .HasMaxLength(100);

        builder.Property(e => e.Address2)
            .HasMaxLength(100);

        builder.Property(e => e.Address3)
            .HasMaxLength(100);

        builder.Property(e => e.Zip)
            .HasMaxLength(20);

        builder.Property(e => e.POBox)
            .HasMaxLength(50);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.Country)
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .HasMaxLength(100);

        builder.Property(e => e.PhoneBusiness)
            .HasMaxLength(100);

        builder.Property(e => e.PhonePrivate)
            .HasMaxLength(100);

        builder.Property(e => e.PhoneMobile)
            .HasMaxLength(100);

        // Configure unique index on ExternalReferenceId
        builder.HasIndex(e => e.ExternalReferenceId)
            .IsUnique()
            .HasFilter($"[{nameof(Contact.ExternalReferenceId)}] IS NOT NULL AND [{nameof(Contact.IsDeleted)}] IS NULL");

        // Configure self-referencing relationship for hierarchy
        builder.HasOne(e => e.ParentContact)
            .WithMany(e => e.ChildContacts)
            .HasForeignKey(e => e.IdParentContact)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
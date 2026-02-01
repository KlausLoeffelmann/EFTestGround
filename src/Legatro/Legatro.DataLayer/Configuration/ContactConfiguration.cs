using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Configuration;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(e => e.IdContact);

        builder.HasIndex(e => e.SyncGuid).IsUnique();
        builder.HasIndex(e => e.ExternalReferenceId).IsUnique();

        builder.Property(e => e.ExternalReferenceId).HasMaxLength(128);
        builder.Property(e => e.Salutation).HasMaxLength(100);
        builder.Property(e => e.MainName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AdditionalName1).HasMaxLength(100);
        builder.Property(e => e.AdditionalName2).HasMaxLength(100);
        builder.Property(e => e.Address1).HasMaxLength(100);
        builder.Property(e => e.Address2).HasMaxLength(100);
        builder.Property(e => e.Address3).HasMaxLength(100);
        builder.Property(e => e.Zip).HasMaxLength(20);
        builder.Property(e => e.POBox).HasMaxLength(50);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.Email).HasMaxLength(100);
        builder.Property(e => e.PhoneBusiness).HasMaxLength(100);
        builder.Property(e => e.PhonePrivate).HasMaxLength(100);
        builder.Property(e => e.PhoneMobile).HasMaxLength(100);

        // Self-reference for organizational hierarchy
        builder.HasOne(e => e.ParentContact)
            .WithMany(e => e.ChildContacts)
            .HasForeignKey(e => e.IdParentContact)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

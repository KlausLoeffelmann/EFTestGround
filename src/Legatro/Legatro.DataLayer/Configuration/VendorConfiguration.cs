using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the Vendor entity.
/// </summary>
public class VendorConfiguration : BaseEntityConfiguration<Vendor>
{
    public override void Configure(EntityTypeBuilder<Vendor> builder)
    {
        base.Configure(builder);

        builder.ToTable("Vendors");

        // Primary key
        builder.HasKey(e => e.IdVendor);

        // Configure IdVendor with default value
        builder.Property(e => e.IdVendor)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.Matchcode)
            .HasMaxLength(50);

        builder.Property(e => e.ExternalReferenceId)
            .HasMaxLength(128);

        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Comment)
            .HasMaxLength(1000);

        // Configure unique index on ExternalReferenceId
        builder.HasIndex(e => e.ExternalReferenceId)
            .IsUnique()
            .HasFilter($"[{nameof(Vendor.ExternalReferenceId)}] IS NOT NULL AND [{nameof(Vendor.IsDeleted)}] IS NULL");

        // Configure numeric property with default
        builder.Property(e => e.VendorNumber)
            .HasDefaultValue(0);

        // Configure boolean properties with defaults
        builder.Property(e => e.IsIndividual)
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        // Configure relationships
        builder.HasOne(e => e.VendorContact)
            .WithOne()
            .HasForeignKey<Vendor>(e => e.IdVendorContact)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MainContact)
            .WithMany()
            .HasForeignKey(e => e.IdMainContact)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure reverse navigation property
        builder.HasMany(e => e.Products)
            .WithOne(e => e.Vendor)
            .HasForeignKey(e => e.IdVendor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
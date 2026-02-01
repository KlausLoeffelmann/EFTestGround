using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the Customer entity.
/// </summary>
public class CustomerConfiguration : BaseEntityConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        builder.ToTable("Customers");

        // Primary key
        builder.HasKey(e => e.IdCustomer);

        // Configure IdCustomer with default value
        builder.Property(e => e.IdCustomer)
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
            .HasFilter($"[{nameof(Customer.ExternalReferenceId)}] IS NOT NULL AND [{nameof(Customer.IsDeleted)}] IS NULL");

        // Configure numeric property with default
        builder.Property(e => e.CustomerNumber)
            .HasDefaultValue(0);

        // Configure boolean properties with defaults
        builder.Property(e => e.IsIndividual)
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        // Configure relationships
        builder.HasOne(e => e.CompanyContact)
            .WithOne()
            .HasForeignKey<Customer>(e => e.IdCompanyContact)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.MainContact)
            .WithMany()
            .HasForeignKey(e => e.IdMainContact)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure reverse navigation property
        builder.HasMany(e => e.Projects)
            .WithOne(e => e.Customer)
            .HasForeignKey(e => e.IdCustomer)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
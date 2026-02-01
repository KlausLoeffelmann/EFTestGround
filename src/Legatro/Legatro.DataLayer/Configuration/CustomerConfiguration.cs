using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.IdCustomer);

        builder.HasIndex(e => e.SyncGuid).IsUnique();
        builder.HasIndex(e => e.ExternalReferenceId).IsUnique();

        builder.Property(e => e.Matchcode).HasMaxLength(50);
        builder.Property(e => e.ExternalReferenceId).HasMaxLength(128);
        builder.Property(e => e.CompanyName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Comment).HasMaxLength(1000);

        // Navigation to company contact
        builder.HasOne(e => e.CompanyContact)
            .WithMany()
            .HasForeignKey(e => e.IdCompanyContact)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation to main contact person
        builder.HasOne(e => e.MainContact)
            .WithMany()
            .HasForeignKey(e => e.IdMainContact)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

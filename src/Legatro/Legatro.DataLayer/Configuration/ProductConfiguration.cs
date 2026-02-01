using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.IdProduct);

        builder.HasIndex(e => e.SyncGuid).IsUnique();

        builder.Property(e => e.ProductName).HasMaxLength(1000);
        builder.Property(e => e.UnitDimension).HasMaxLength(50);

        // Store price as money type
        builder.Property(e => e.UnitMainPrice).HasColumnType("decimal(19,4)");
        builder.Property(e => e.VAT).HasColumnType("decimal(9,4)");

        // Navigation to vendor
        builder.HasOne(e => e.Vendor)
            .WithMany(v => v.Products)
            .HasForeignKey(e => e.IdVendor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the Product entity.
/// </summary>
public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products");

        // Primary key
        builder.HasKey(e => e.IdProduct);

        // Configure IdProduct with default value
        builder.Property(e => e.IdProduct)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.ProductName)
            .HasMaxLength(1000);

        builder.Property(e => e.UnitDimension)
            .HasMaxLength(50);

        // Configure ProductImage as image/varbinary
        builder.Property(e => e.ProductImage);

        // Configure decimal properties
        builder.Property(e => e.QuantityPerUnit);
        builder.Property(e => e.UnitsInStock);
        builder.Property(e => e.UnitsInProduction);
        builder.Property(e => e.UnitsAtCustomer);
        builder.Property(e => e.TeHMin);
        builder.Property(e => e.UnitMainPrice);
        builder.Property(e => e.VAT);

        // Configure boolean properties with defaults
        builder.Property(e => e.Discontinued)
            .HasDefaultValue(false);

        // Configure nullable boolean property
        builder.Property(e => e.ReturningProductService);

        // Configure relationship
        builder.HasOne(e => e.Vendor)
            .WithMany(v => v.Products)
            .HasForeignKey(e => e.IdVendor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
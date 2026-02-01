using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the Category entity.
/// </summary>
public class CategoryConfiguration : BaseEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.ToTable("Categories");

        // Primary key
        builder.HasKey(e => e.IdCategory);

        // Configure IdCategory with default value
        builder.Property(e => e.IdCategory)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.CategoryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CategoryDescription)
            .HasMaxLength(2000);

        // Configure boolean property with default
        builder.Property(e => e.IsSystemCategory)
            .HasDefaultValue(false);

        // Configure relationship
        builder.HasMany(e => e.TimeItems)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.IdTimeItemCategory)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
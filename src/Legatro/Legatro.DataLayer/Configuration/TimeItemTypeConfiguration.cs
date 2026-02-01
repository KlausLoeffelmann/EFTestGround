using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the TimeItemType entity.
/// </summary>
public class TimeItemTypeConfiguration : BaseEntityConfiguration<TimeItemType>
{
    public override void Configure(EntityTypeBuilder<TimeItemType> builder)
    {
        base.Configure(builder);

        builder.ToTable("TimeItemTypes");

        // Primary key
        builder.HasKey(e => e.IdTimeItemType);

        // Configure IdTimeItemType with default value
        builder.Property(e => e.IdTimeItemType)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.TimeItemTypeName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ShortName)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(255);

        // Configure BookingType as short
        builder.Property(e => e.BookingType)
            .IsRequired();

        // Configure boolean property with default
        builder.Property(e => e.IsSystemType)
            .HasDefaultValue(false);

        // Configure relationship
        builder.HasMany(e => e.TimeItems)
            .WithOne(e => e.TimeItemType)
            .HasForeignKey(e => e.IdTimeItemType)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
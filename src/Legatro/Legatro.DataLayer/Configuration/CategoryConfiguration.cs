using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(e => e.IdCategory);

        builder.HasIndex(e => e.SyncGuid).IsUnique();

        builder.Property(e => e.CategoryName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.CategoryDescription).HasMaxLength(2000);
    }
}

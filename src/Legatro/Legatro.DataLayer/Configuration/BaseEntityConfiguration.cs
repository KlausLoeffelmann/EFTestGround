using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Base configuration for all entities that inherit from BaseEntity.
/// </summary>
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // Configure SyncGuid as unique
        builder.Property(e => e.SyncGuid)
            .IsRequired()
            .HasDefaultValueSql("newsequentialid()");

        builder.HasIndex(e => e.SyncGuid)
            .IsUnique()
            .HasFilter($"[{nameof(BaseEntity.IsDeleted)}] IS NULL");

        // Configure DateCreated with default value
        builder.Property(e => e.DateCreated)
            .IsRequired()
            .HasDefaultValueSql("getutcdate()");

        // Configure DateLastEdited with default value
        builder.Property(e => e.DateLastEdited)
            .IsRequired()
            .HasDefaultValueSql("getutcdate()");

        // Configure IsDeleted for soft delete
        builder.Property(e => e.IsDeleted)
            .IsRequired(false);

        // Configure query filter to exclude soft-deleted entities
        builder.HasQueryFilter(e => e.IsDeleted == null);
    }
}
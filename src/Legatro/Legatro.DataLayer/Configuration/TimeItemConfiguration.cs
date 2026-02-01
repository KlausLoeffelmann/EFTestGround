using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.ValueConverters;

namespace Legatro.DataLayer.Configuration;

public class TimeItemConfiguration : IEntityTypeConfiguration<TimeItem>
{
    public void Configure(EntityTypeBuilder<TimeItem> builder)
    {
        builder.HasKey(e => e.IdTimeItem);

        #region Indexes

        // Unique indexes
        builder.HasIndex(e => e.SyncGuid).IsUnique();
        builder.HasIndex(e => e.ExternalReferenceId).IsUnique();

        // Performance indexes
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.EventTime);
        builder.HasIndex(e => e.BookingDateGMT);
        builder.HasIndex(e => e.IsCompleted);
        builder.HasIndex(e => e.IsDeleted);

        // Composite index for linked-list queries (most important for performance)
        builder.HasIndex(e => new { e.IdUser, e.BookingDateGMT, e.EventTime });

        // Index for finding active (non-archived) items
        builder.HasIndex(e => new { e.IdUser, e.BookingDateGMT, e.DateValidTo });

        #endregion

        #region Property Configuration

        // Ignore spatial type property - requires database-specific NTS support
        builder.Ignore(e => e.Location);

        builder.Property(e => e.EventInfo).HasMaxLength(100);
        builder.Property(e => e.ShortTitel).HasMaxLength(2000);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.MetaInfo).HasColumnType("text");
        builder.Property(e => e.ExternalReferenceId).HasMaxLength(128);
        builder.Property(e => e.DeviceInfo).HasMaxLength(255);
        builder.Property(e => e.LocationInfo).HasMaxLength(255);

        // Store EventType as int
        builder.Property(e => e.EventType)
            .HasConversion<int>();

        // Use value converter for TimeSpan fields
        var timeSpanConverter = new TimeSpanToTicksConverter();
        builder.Property(e => e.DurationToNext)
            .HasConversion(timeSpanConverter);
        builder.Property(e => e.DurationToPrevious)
            .HasConversion(timeSpanConverter);

        #endregion

        #region Navigation Properties - Foreign Keys

        // User (required)
        builder.HasOne(e => e.User)
            .WithMany(u => u.TimeItems)
            .HasForeignKey(e => e.IdUser)
            .OnDelete(DeleteBehavior.Restrict);

        // Requesting User (optional)
        builder.HasOne(e => e.RequestingUser)
            .WithMany()
            .HasForeignKey(e => e.IdUserItemFrom)
            .OnDelete(DeleteBehavior.Restrict);

        // Project (optional)
        builder.HasOne(e => e.Project)
            .WithMany(p => p.TimeItems)
            .HasForeignKey(e => e.IdProject)
            .OnDelete(DeleteBehavior.Restrict);

        // Task (optional)
        builder.HasOne(e => e.Task)
            .WithMany(t => t.TimeItems)
            .HasForeignKey(e => e.IdTask)
            .OnDelete(DeleteBehavior.Restrict);

        // Category (optional)
        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.IdTimeItemCategory)
            .OnDelete(DeleteBehavior.Restrict);

        // TimeItemType (optional)
        builder.HasOne(e => e.TimeItemType)
            .WithMany()
            .HasForeignKey(e => e.IdTimeItemType)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        #region Self-References

        // History Parent (versioning) - one-to-many
        builder.HasOne(e => e.HistoryParent)
            .WithMany(e => e.HistoryChildren)
            .HasForeignKey(e => e.IdHistoryParent)
            .OnDelete(DeleteBehavior.Restrict);

        // IdNextItem and IdPreviousItem are NOT FK relationships
        // They are manually-managed linked list pointers stored as plain Guid columns
        // No navigation properties, no FK constraints - just indexed columns for performance
        builder.HasIndex(e => e.IdNextItem);
        builder.HasIndex(e => e.IdPreviousItem);

        // Parent TimeItem (hierarchical) - one-to-many
        builder.HasOne(e => e.ParentTimeItem)
            .WithMany(e => e.ChildTimeItems)
            .HasForeignKey(e => e.IdParentTimeItem)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}

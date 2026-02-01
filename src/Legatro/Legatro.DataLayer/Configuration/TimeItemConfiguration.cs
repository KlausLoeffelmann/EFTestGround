using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the TimeItem entity.
/// </summary>
public class TimeItemConfiguration : IEntityTypeConfiguration<TimeItem>
{
    public void Configure(EntityTypeBuilder<TimeItem> builder)
    {
        builder.ToTable("TimeItems");

        // Primary key
        builder.HasKey(e => e.IdTimeItem);

        // Configure IdTimeItem with default value
        builder.Property(e => e.IdTimeItem)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.EventInfo)
            .HasMaxLength(100);

        builder.Property(e => e.ShortTitel)
            .HasMaxLength(2000);

        builder.Property(e => e.ExternalReferenceId)
            .HasMaxLength(128);

        builder.Property(e => e.DeviceInfo)
            .HasMaxLength(255);

        builder.Property(e => e.LocationInfo)
            .HasMaxLength(255);

        builder.Property(e => e.MetaInfo)
            .HasColumnType("text");

        builder.Property(e => e.Description)
            .HasColumnType("text");

        // Configure EventType as short
        builder.Property(e => e.EventType)
            .HasConversion<short>()
            .IsRequired();

        // Configure indexes
        builder.HasIndex(e => e.EventType);

        builder.HasIndex(e => new { e.BookingDateGMT, e.IdUser });

        builder.HasIndex(e => e.EventTime);

        builder.HasIndex(e => e.IsCompleted);

        builder.HasIndex(e => e.IsDeleted);

        builder.HasIndex(e => e.ExternalReferenceId)
            .IsUnique()
            .HasFilter($"[{nameof(TimeItem.ExternalReferenceId)}] IS NOT NULL AND [{nameof(TimeItem.IsDeleted)}] = 0");

        // Configure numeric properties
        builder.Property(e => e.Priority)
            .HasDefaultValue(1000);

        builder.Property(e => e.SortOrder)
            .HasDefaultValue(0.0);

        // Configure boolean properties with defaults
        builder.Property(e => e.IsTaggedForDueNotification)
            .HasDefaultValue(false);

        builder.Property(e => e.IsNewQuickItem)
            .HasDefaultValue(false);

        builder.Property(e => e.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(e => e.IsStartAction)
            .HasDefaultValue(false);

        builder.Property(e => e.IsEndAction)
            .HasDefaultValue(false);

        builder.Property(e => e.IsAssignmentRejected)
            .HasDefaultValue(false);

        // Configure DateTime properties
        builder.Property(e => e.DateCreated)
            .IsRequired()
            .HasDefaultValueSql("getutcdate()");

        builder.Property(e => e.DateLastEdited)
            .IsRequired()
            .HasDefaultValueSql("getutcdate()");

        builder.Property(e => e.DateValidTo)
            .IsRequired(false);

        // Configure SyncGuid
        builder.Property(e => e.SyncGuid)
            .IsRequired()
            .HasDefaultValueSql("newsequentialid()");

        builder.HasIndex(e => e.SyncGuid)
            .IsUnique();

        // Configure query filter to exclude soft-deleted items
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Configure relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.TimeItems)
            .HasForeignKey(e => e.IdUser)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Project)
            .WithMany(p => p.TimeItems)
            .HasForeignKey(e => e.IdProject)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Task)
            .WithMany(t => t.TimeItems)
            .HasForeignKey(e => e.IdTask)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.TimeItems)
            .HasForeignKey(e => e.IdTimeItemCategory)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TimeItemType)
            .WithMany(t => t.TimeItems)
            .HasForeignKey(e => e.IdTimeItemType)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationships for linked list
        builder.HasOne(e => e.NextItem)
            .WithMany(e => e.PreviousItems)
            .HasForeignKey(e => e.IdNextItem)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PreviousItem)
            .WithMany()
            .HasForeignKey(e => e.IdPreviousItem)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing for history parent
        builder.HasOne(e => e.HistoryParent)
            .WithMany(e => e.HistoryVersions)
            .HasForeignKey(e => e.IdHistoryParent)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing for parent time item
        builder.HasOne(e => e.ParentTimeItem)
            .WithMany(e => e.ChildTimeItems)
            .HasForeignKey(e => e.IdParentTimeItem)
            .OnDelete(DeleteBehavior.Restrict);

        // User who requested the item
        builder.HasOne(e => e.RequestedBy)
            .WithMany()
            .HasForeignKey(e => e.IdUserItemFrom)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure value converters for TimeSpan (stored as ticks)
        var timeSpanConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<TimeSpan?, long?>(
            v => v.HasValue ? v.Value.Ticks : (long?)null,
            v => v.HasValue ? TimeSpan.FromTicks(v.Value) : (TimeSpan?)null);

        builder.Property(e => e.DurationToNext)
            .HasConversion(timeSpanConverter);

        builder.Property(e => e.DurationToPrevious)
            .HasConversion(timeSpanConverter);

        // Configure value converters for DateTimeOffset (stored as DateTime)
        var dateTimeOffsetConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset?, DateTime?>(
            v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
            v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);

        builder.Property(e => e.ItemCompletedRequestDate)
            .HasConversion(dateTimeOffsetConverter);

        builder.Property(e => e.DateItemAcceptedOrRejected)
            .HasConversion(dateTimeOffsetConverter);

        builder.Property(e => e.DateItemFinished)
            .HasConversion(dateTimeOffsetConverter);

        builder.Property(e => e.LastNotificationSentDate)
            .HasConversion(dateTimeOffsetConverter);

        builder.Property(e => e.NotificationAcknowledgedDate)
            .HasConversion(dateTimeOffsetConverter);

        builder.Property(e => e.EventTime)
            .HasConversion(dateTimeOffsetConverter);
    }
}
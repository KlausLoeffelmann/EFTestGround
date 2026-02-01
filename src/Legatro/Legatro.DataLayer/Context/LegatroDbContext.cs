using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NetTopologySuite.Geometries;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.DataLayer.Context;

/// <summary>
/// Main database context for the Legatro application.
/// </summary>
public class LegatroDbContext : DbContext
{
    public LegatroDbContext(DbContextOptions<LegatroDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<TimeItem> TimeItems => Set<TimeItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LegatroTask> Tasks => Set<LegatroTask>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TimeItemType> TimeItemTypes => Set<TimeItemType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LegatroDbContext).Assembly);

        // Configure value converters
        ConfigureValueConverters(modelBuilder);
    }

    private void ConfigureValueConverters(ModelBuilder modelBuilder)
    {
        // Convert TimeSpan to long (ticks) for storage
        var timeSpanToTicksConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<TimeSpan, long>(
            v => v.Ticks,
            v => TimeSpan.FromTicks(v));

        // Convert DateTimeOffset to DateTime for storage
        var dateTimeOffsetToDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, DateTime>(
            v => v.UtcDateTime,
            v => new DateTimeOffset(v, TimeSpan.Zero));

        // Apply TimeSpan converter to TimeItem properties
        modelBuilder.Entity<TimeItem>()
            .Property(e => e.DurationToNext)
            .HasConversion(timeSpanToTicksConverter);

        modelBuilder.Entity<TimeItem>()
            .Property(e => e.DurationToPrevious)
            .HasConversion(timeSpanToTicksConverter);

        // Apply DateTimeOffset converter to TimeItem properties
        modelBuilder.Entity<TimeItem>()
            .Property(e => e.ItemCompletedRequestDate)
            .HasConversion(dateTimeOffsetToDateTimeConverter);

        modelBuilder.Entity<TimeItem>()
            .Property(e => e.DateItemAcceptedOrRejected)
            .HasConversion(dateTimeOffsetToDateTimeConverter);

        modelBuilder.Entity<TimeItem>()
            .Property(e => e.DateItemFinished)
            .HasConversion(dateTimeOffsetToDateTimeConverter);

        modelBuilder.Entity<TimeItem>()
            .Property(e => e.LastNotificationSentDate)
            .HasConversion(dateTimeOffsetToDateTimeConverter);

        modelBuilder.Entity<TimeItem>()
            .Property(e => e.NotificationAcknowledgedDate)
            .HasConversion(dateTimeOffsetToDateTimeConverter);

        modelBuilder.Entity<TimeItem>()
            .Property(e => e.EventTime)
            .HasConversion(dateTimeOffsetToDateTimeConverter);

        // Convert EventType enum to short
        modelBuilder.Entity<TimeItem>()
            .Property(e => e.EventType)
            .HasConversion<short>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries();

        // Handle audit fields for BaseEntity-derived entities
        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.DateCreated = DateTime.UtcNow;
                        baseEntity.DateLastEdited = DateTime.UtcNow;
                        baseEntity.SyncGuid = Guid.NewGuid();
                        break;

                    case EntityState.Modified:
                        if (entry.Property(nameof(BaseEntity.DateCreated)).IsModified)
                        {
                            entry.Property(nameof(BaseEntity.DateCreated)).IsModified = false;
                        }
                        baseEntity.DateLastEdited = DateTime.UtcNow;
                        baseEntity.SyncGuid = Guid.NewGuid();
                        break;

                    case EntityState.Deleted:
                        // Soft delete: mark as deleted instead of physical delete
                        entry.State = EntityState.Modified;
                        baseEntity.IsDeleted = DateTime.UtcNow;
                        baseEntity.DateLastEdited = DateTime.UtcNow;
                        baseEntity.SyncGuid = Guid.NewGuid();
                        break;
                }
            }

            // Handle TimeItem audit fields separately (TimeItem doesn't inherit from BaseEntity)
            if (entry.Entity is TimeItem timeItem)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        timeItem.DateCreated = DateTime.UtcNow;
                        timeItem.DateLastEdited = DateTime.UtcNow;
                        timeItem.SyncGuid = Guid.NewGuid();
                        break;

                    case EntityState.Modified:
                        if (entry.Property(nameof(TimeItem.DateCreated)).IsModified)
                        {
                            entry.Property(nameof(TimeItem.DateCreated)).IsModified = false;
                        }
                        timeItem.DateLastEdited = DateTime.UtcNow;
                        timeItem.SyncGuid = Guid.NewGuid();
                        break;

                    case EntityState.Deleted:
                        // Soft delete: mark as deleted instead of physical delete
                        entry.State = EntityState.Modified;
                        timeItem.IsDeleted = true;
                        timeItem.DateLastEdited = DateTime.UtcNow;
                        timeItem.SyncGuid = Guid.NewGuid();
                        break;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
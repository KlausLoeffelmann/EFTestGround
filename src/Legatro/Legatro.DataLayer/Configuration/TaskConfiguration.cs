using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the Task entity.
/// </summary>
public class TaskConfiguration : BaseEntityConfiguration<LegatroTask>
{
    public override void Configure(EntityTypeBuilder<LegatroTask> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tasks");

        // Primary key
        builder.HasKey(e => e.IdTask);

        // Configure IdTask with default value
        builder.Property(e => e.IdTask)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.ExternalReferenceId)
            .HasMaxLength(128);

        builder.Property(e => e.TaskName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.TaskDescription)
            .IsRequired()
            .HasMaxLength(3000);

        // Configure unique index on ExternalReferenceId
        builder.HasIndex(e => e.ExternalReferenceId)
            .IsUnique()
            .HasFilter($"[{nameof(LegatroTask.ExternalReferenceId)}] IS NOT NULL AND [{nameof(LegatroTask.IsDeleted)}] IS NULL");

        // Configure numeric properties with defaults
        builder.Property(e => e.TaskOrderNo)
            .HasDefaultValue(0);

        // Configure boolean properties with defaults
        builder.Property(e => e.TaskDone)
            .HasDefaultValue(false);

        builder.Property(e => e.IsTemplate)
            .HasDefaultValue(false);

        // Configure nullable numeric properties
        builder.Property(e => e.TaskNo);
        builder.Property(e => e.TimeInMinutesWorkedOn);
        builder.Property(e => e.PlanedCapacityInMinutes);
        builder.Property(e => e.RememberMinutesBefore);
        builder.Property(e => e.TimePerUnit);
        builder.Property(e => e.DegreeOfDifficultyProMille);

        // Configure DateTime properties
        builder.Property(e => e.DueDate);
        builder.Property(e => e.TaskDoneDate);

        // Configure self-referencing relationship for hierarchy
        builder.HasOne(e => e.ParentTask)
            .WithMany(e => e.ChildTasks)
            .HasForeignKey(e => e.IdParentTask)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationships
        builder.HasOne(e => e.TaskCreatedBy)
            .WithMany()
            .HasForeignKey(e => e.IdUserTaskCreated)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Owner)
            .WithMany(u => u.OwnedTasks)
            .HasForeignKey(e => e.IdUserAsOwner)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(e => e.IdProject)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure reverse navigation property
        builder.HasMany(e => e.TimeItems)
            .WithOne(e => e.Task)
            .HasForeignKey(e => e.IdTask)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
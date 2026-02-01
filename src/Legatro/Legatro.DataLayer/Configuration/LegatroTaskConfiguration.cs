using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Configuration;

public class LegatroTaskConfiguration : IEntityTypeConfiguration<LegatroTask>
{
    public void Configure(EntityTypeBuilder<LegatroTask> builder)
    {
        // Map to "Tasks" table to maintain traditional naming
        builder.ToTable("Tasks");

        builder.HasKey(e => e.IdTask);

        builder.HasIndex(e => e.SyncGuid).IsUnique();
        builder.HasIndex(e => e.ExternalReferenceId).IsUnique();

        builder.Property(e => e.ExternalReferenceId).HasMaxLength(128);
        builder.Property(e => e.TaskName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.TaskDescription).HasMaxLength(3000).IsRequired();

        // Self-reference for task hierarchy
        builder.HasOne(e => e.ParentTask)
            .WithMany(e => e.SubTasks)
            .HasForeignKey(e => e.IdParentTask)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation to project
        builder.HasOne(e => e.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(e => e.IdProject)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation to owner
        builder.HasOne(e => e.Owner)
            .WithMany(u => u.OwnedTasks)
            .HasForeignKey(e => e.IdUserAsOwner)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation to creator
        builder.HasOne(e => e.Creator)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(e => e.IdUserTaskCreated)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the Project entity.
/// </summary>
public class ProjectConfiguration : BaseEntityConfiguration<Project>
{
    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);

        builder.ToTable("Projects");

        // Primary key
        builder.HasKey(e => e.IdProject);

        // Configure IdProject with default value
        builder.Property(e => e.IdProject)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.ExternalReferenceId)
            .HasMaxLength(128);

        builder.Property(e => e.ProjectName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ShortProjectName)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.Description)
            .HasMaxLength(3000);

        // Configure unique index on ExternalReferenceId
        builder.HasIndex(e => e.ExternalReferenceId)
            .IsUnique()
            .HasFilter($"[{nameof(Project.ExternalReferenceId)}] IS NOT NULL AND [{nameof(Project.IsDeleted)}] IS NULL");

        // Configure numeric properties with defaults
        builder.Property(e => e.ProjectNumber)
            .HasDefaultValue(0);

        // Configure boolean properties with defaults
        builder.Property(e => e.IsProject)
            .HasDefaultValue(true);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.IsSubProject)
            .HasDefaultValue(false);

        builder.Property(e => e.CollectProductionUnits)
            .HasDefaultValue(false);

        builder.Property(e => e.CollectProductionUnitsDescription)
            .HasDefaultValue(false);

        builder.Property(e => e.HasCustomers)
            .HasDefaultValue(false);

        // Configure nullable properties
        builder.Property(e => e.MonitorTimeCapacity);
        builder.Property(e => e.MonthlyTargetTimeCapacity);
        builder.Property(e => e.MonitorStartdate);
        builder.Property(e => e.MonitorEnddate);
        builder.Property(e => e.TotalTargetTimeCapacity);
        builder.Property(e => e.MaxBookedEmployees);

        // Configure self-referencing relationship for hierarchy
        builder.HasOne(e => e.ParentProject)
            .WithMany(e => e.ChildProjects)
            .HasForeignKey(e => e.IdParentProject)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationships
        builder.HasOne(e => e.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(e => e.IdUserAsOwner)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(e => e.IdCustomer)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure reverse navigation properties
        builder.HasMany(e => e.TimeItems)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.IdProject)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Tasks)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.IdProject)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
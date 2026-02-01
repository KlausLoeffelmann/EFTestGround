using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Configuration;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(e => e.IdProject);

        builder.HasIndex(e => e.SyncGuid).IsUnique();
        builder.HasIndex(e => e.ExternalReferenceId).IsUnique();

        builder.Property(e => e.ExternalReferenceId).HasMaxLength(128);
        builder.Property(e => e.ProjectName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.ShortProjectName).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(3000);

        // Navigation to owner
        builder.HasOne(e => e.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(e => e.IdUserAsOwner)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-reference for project hierarchy
        builder.HasOne(e => e.ParentProject)
            .WithMany(e => e.SubProjects)
            .HasForeignKey(e => e.IdParentProject)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation to customer
        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(e => e.IdCustomer)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

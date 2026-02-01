using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;

namespace Legatro.DataLayer.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.IdUser);

        builder.HasIndex(e => e.SyncGuid).IsUnique();
        builder.HasIndex(e => e.ExternalReferenceId).IsUnique();

        builder.Property(e => e.ExternalReferenceId).HasMaxLength(128);
        builder.Property(e => e.PersonnelNumber).HasMaxLength(50);
        builder.Property(e => e.Matchcode).HasMaxLength(50);
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.MiddleName).HasMaxLength(100);
        builder.Property(e => e.Username).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Password).HasMaxLength(128);
        builder.Property(e => e.RfId).HasMaxLength(50);
        builder.Property(e => e.TimeCardNo).HasMaxLength(255);
        builder.Property(e => e.Comment).HasMaxLength(2000);

        // Navigation to contact
        builder.HasOne(e => e.Contact)
            .WithMany()
            .HasForeignKey(e => e.IdContact)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-reference for shadow user relationship
        builder.HasOne(e => e.ShadowTarget)
            .WithMany(e => e.ShadowUsers)
            .HasForeignKey(e => e.IdUserForShadow)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Legatro.DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Legatro.DataLayer.Configuration;

/// <summary>
/// Configuration for the User entity.
/// </summary>
public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        // Primary key
        builder.HasKey(e => e.IdUser);

        // Configure IdUser with default value
        builder.Property(e => e.IdUser)
            .HasDefaultValueSql("newsequentialid()");

        // Configure string properties with max lengths
        builder.Property(e => e.ExternalReferenceId)
            .HasMaxLength(128);

        builder.Property(e => e.PersonnelNumber)
            .HasMaxLength(50);

        builder.Property(e => e.Matchcode)
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.MiddleName)
            .HasMaxLength(100);

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.RfId)
            .HasMaxLength(50);

        builder.Property(e => e.TimeCardNo)
            .HasMaxLength(255);

        builder.Property(e => e.Comment)
            .HasMaxLength(2000);

        // Configure Password as varbinary
        builder.Property(e => e.Password)
            .HasMaxLength(128);

        // Configure unique index on ExternalReferenceId
        builder.HasIndex(e => e.ExternalReferenceId)
            .IsUnique()
            .HasFilter($"[{nameof(User.ExternalReferenceId)}] IS NOT NULL AND [{nameof(User.IsDeleted)}] IS NULL");

        // Configure index on Username
        builder.HasIndex(e => e.Username);

        // Configure numeric properties
        builder.Property(e => e.ClearanceLevel)
            .HasDefaultValue(0);

        // Configure boolean properties with defaults
        builder.Property(e => e.IsShadowEmployee)
            .HasDefaultValue(false);

        builder.Property(e => e.AutoBookForShadowImplicitly)
            .HasDefaultValue(false);

        builder.Property(e => e.IsAdmin)
            .HasDefaultValue(false);

        builder.Property(e => e.IsActivated)
            .HasDefaultValue(true);

        builder.Property(e => e.IsSystemAccount)
            .HasDefaultValue(false);

        // Configure relationships
        builder.HasOne(e => e.Contact)
            .WithOne()
            .HasForeignKey<User>(e => e.IdContact)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing for shadow employee
        builder.HasOne(e => e.ShadowUser)
            .WithMany(e => e.ShadowingUsers)
            .HasForeignKey(e => e.IdUserForShadow)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
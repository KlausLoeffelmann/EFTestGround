using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Entities.Base;

namespace Legatro.DataLayer.Context;

/// <summary>
/// The main database context for the Legatro application.
/// Provides automatic auditing through SaveChanges override.
/// </summary>
public class LegatroDbContext : DbContext
{
    public LegatroDbContext(DbContextOptions<LegatroDbContext> options) : base(options)
    {
    }

    #region DbSets

    public DbSet<TimeItem> TimeItems => Set<TimeItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<LegatroTask> Tasks => Set<LegatroTask>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TimeItemType> TimeItemTypes => Set<TimeItemType>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LegatroDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditingRules();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditingRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditingRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditingRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Applies auditing rules to all tracked entities.
    /// On Add: Set DateCreated, DateLastEdited to UtcNow; generate new SyncGuid
    /// On Modify: Update DateLastEdited to UtcNow; regenerate SyncGuid
    /// </summary>
    private void ApplyAuditingRules()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SyncGuid = Guid.NewGuid();
                    entry.Entity.DateCreated = utcNow;
                    entry.Entity.DateLastEdited = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.SyncGuid = Guid.NewGuid();
                    entry.Entity.DateLastEdited = utcNow;
                    break;
            }
        }
    }
}

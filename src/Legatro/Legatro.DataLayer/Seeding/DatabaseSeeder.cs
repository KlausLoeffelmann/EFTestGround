using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace Legatro.DataLayer.Seeding;

/// <summary>
/// Seeds default data into the database.
/// </summary>
public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly LegatroDbContext _context;
    private readonly LegatroDbContextFactory _factory;

    public DatabaseSeeder(LegatroDbContext context, LegatroDbContextFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// Seeds all default data into the database.
    /// </summary>
    public async System.Threading.Tasks.Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        await SeedTimeItemTypesAsync(cancellationToken);
        await SeedSystemUserAsync(cancellationToken);
        await SeedDefaultCategoriesAsync(cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Seeds default TimeItemTypes into the database.
    /// </summary>
    public async System.Threading.Tasks.Task SeedTimeItemTypesAsync(CancellationToken cancellationToken = default)
    {
        var existingTypes = await _context.TimeItemTypes
            .AsNoTracking()
            .Where(t => t.IsSystemType)
            .ToListAsync(cancellationToken);

        var systemTypes = GetDefaultTimeItemTypes();

        foreach (var type in systemTypes)
        {
            if (!existingTypes.Any(t => t.TimeItemTypeName == type.TimeItemTypeName))
            {
                type.DateCreated = DateTime.UtcNow;
                type.DateLastEdited = DateTime.UtcNow;
                type.SyncGuid = Guid.NewGuid();
                await _context.TimeItemTypes.AddAsync(type, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Gets the default TimeItemTypes defined in the schema.
    /// </summary>
    private static List<TimeItemType> GetDefaultTimeItemTypes()
    {
        return new List<TimeItemType>
        {
            new()
            {
                IdTimeItemType = Guid.NewGuid(),
                TimeItemTypeName = "Default",
                ShortName = "Default",
                DisplayOrder = 0,
                IsSystemType = true,
                BookingType = (short)TimeItemBookingType.Default,
                Description = "Used in the flat time table for all entries related to quantity capture timestamps."
            },
            new()
            {
                IdTimeItemType = Guid.NewGuid(),
                TimeItemTypeName = "Check In",
                ShortName = "CheckIn",
                DisplayOrder = 1,
                IsSystemType = true,
                BookingType = (short)TimeItemBookingType.CheckIn,
                Description = "Called when an employee clocks in."
            },
            new()
            {
                IdTimeItemType = Guid.NewGuid(),
                TimeItemTypeName = "Check Out",
                ShortName = "CheckOut",
                DisplayOrder = 2,
                IsSystemType = true,
                BookingType = (short)TimeItemBookingType.CheckOut,
                Description = "Called when an employee clocks out."
            },
            new()
            {
                IdTimeItemType = Guid.NewGuid(),
                TimeItemTypeName = "Break",
                ShortName = "Break",
                DisplayOrder = 3,
                IsSystemType = true,
                BookingType = (short)TimeItemBookingType.Break,
                Description = "Employee books an unspecified break."
            },
            new()
            {
                IdTimeItemType = Guid.NewGuid(),
                TimeItemTypeName = "Downtime",
                ShortName = "Downtime",
                DisplayOrder = 4,
                IsSystemType = true,
                BookingType = (short)TimeItemBookingType.Downtime,
                Description = "Downtime booked when employee cannot continue working for operational reasons."
            },
            new()
            {
                IdTimeItemType = Guid.NewGuid(),
                TimeItemTypeName = "Business Errand",
                ShortName = "BizErrand",
                DisplayOrder = 5,
                IsSystemType = true,
                BookingType = (short)TimeItemBookingType.BusinessErrand,
                Description = "Unspecified business errand outside the office."
            },
            new()
            {
                IdTimeItemType = Guid.NewGuid(),
                TimeItemTypeName = "Set Booking",
                ShortName = "SetBooking",
                DisplayOrder = 6,
                IsSystemType = true,
                BookingType = (short)TimeItemBookingType.SetBooking,
                Description = "Booking to a target set that can contain multiple projects, orders, products, or combinations."
            }
        };
    }

    /// <summary>
    /// Seeds a system user into the database.
    /// </summary>
    public async System.Threading.Tasks.Task SeedSystemUserAsync(CancellationToken cancellationToken = default)
    {
        var existingSystemUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IsSystemAccount && u.Username == "system", cancellationToken);

        if (existingSystemUser != null)
        {
            return;
        }

        // Create a contact for the system user
        var contact = new Contact
        {
            IdContact = Guid.NewGuid(),
            MainName = "System",
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };

        await _context.Contacts.AddAsync(contact, cancellationToken);

        // Create the system user
        var systemUser = new User
        {
            IdUser = Guid.NewGuid(),
            IdContact = contact.IdContact,
            Username = "system",
            FirstName = "System",
            LastName = "Account",
            IsActivated = true,
            IsSystemAccount = true,
            ClearanceLevel = 0,
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };

        await _context.Users.AddAsync(systemUser, cancellationToken);
    }

    /// <summary>
    /// Seeds default categories into the database.
    /// </summary>
    public async System.Threading.Tasks.Task SeedDefaultCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var existingCategories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsSystemCategory)
            .ToListAsync(cancellationToken);

        var systemCategories = GetDefaultCategories();

        foreach (var category in systemCategories)
        {
            if (!existingCategories.Any(c => c.CategoryName == category.CategoryName))
            {
                category.DateCreated = DateTime.UtcNow;
                category.DateLastEdited = DateTime.UtcNow;
                category.SyncGuid = Guid.NewGuid();
                await _context.Categories.AddAsync(category, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Gets the default categories.
    /// </summary>
    private static List<Category> GetDefaultCategories()
    {
        return new List<Category>
        {
            new()
            {
                IdCategory = Guid.NewGuid(),
                CategoryName = "General",
                CategoryDescription = "General category for uncategorized items.",
                IsSystemCategory = true
            },
            new()
            {
                IdCategory = Guid.NewGuid(),
                CategoryName = "Development",
                CategoryDescription = "Software development related tasks.",
                IsSystemCategory = true
            },
            new()
            {
                IdCategory = Guid.NewGuid(),
                CategoryName = "Design",
                CategoryDescription = "Design and UI/UX related tasks.",
                IsSystemCategory = true
            },
            new()
            {
                IdCategory = Guid.NewGuid(),
                CategoryName = "Administration",
                CategoryDescription = "Administrative and management tasks.",
                IsSystemCategory = true
            },
            new()
            {
                IdCategory = Guid.NewGuid(),
                CategoryName = "Meeting",
                CategoryDescription = "Meeting and communication tasks.",
                IsSystemCategory = true
            }
        };
    }
}
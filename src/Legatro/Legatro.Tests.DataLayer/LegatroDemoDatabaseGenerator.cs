using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.Tests.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.Tests.DataLayer;

/// <summary>
/// Generates a realistic demo SQLite database for testing and development.
/// </summary>
public class LegatroDemoDatabaseGenerator
{
    private readonly DemoDataConfiguration _config;
    private readonly LegatroDbContextFactory _factory;
    private readonly string _databasePath;

    /// <summary>
    /// Creates a new generator with its own database connection.
    /// </summary>
    public LegatroDemoDatabaseGenerator(DemoDataConfiguration? config = null)
    {
        _config = config ?? new DemoDataConfiguration();
        _databasePath = GetDatabasePath();
        _factory = LegatroDbContextFactory.CreateForSqlite(
            $"Data Source={_databasePath}",
            enableSensitiveDataLogging: false);
    }

    /// <summary>
    /// Creates a new generator that uses an existing factory and database.
    /// </summary>
    public LegatroDemoDatabaseGenerator(DemoDataConfiguration config, LegatroDbContextFactory factory)
    {
        _config = config;
        _databasePath = string.Empty; // Not used when factory is provided
        _factory = factory;
    }

    /// <summary>
    /// Generates a complete demo database with the specified configuration.
    /// </summary>
    public async System.Threading.Tasks.Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        // Ensure database is created
        using var context = _factory.CreateDbContext();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        // Clear existing data
        await ClearDatabaseAsync(context, cancellationToken);

        // Seed base data (TimeItemTypes, system user, categories)
        var seeder = new Legatro.DataLayer.Seeding.DatabaseSeeder(context, _factory);
        await seeder.SeedAllAsync(cancellationToken);

        // Generate demo data
        var users = await GenerateUsersAsync(context, cancellationToken);
        var customers = await GenerateCustomersAsync(context, cancellationToken);

        await GenerateProjectsAsync(context, users, customers, cancellationToken);
        await GenerateTimeDataAsync(context, users, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Clears all data and regenerates the database.
    /// </summary>
    public async System.Threading.Tasks.Task RegenerateAsync(DemoDataConfiguration? config = null, CancellationToken cancellationToken = default)
    {
        if (config != null)
        {
            _config.NumberOfUsers = config.NumberOfUsers;
            _config.ProjectsPerUser = config.ProjectsPerUser;
            _config.TasksPerProject = config.TasksPerProject;
            _config.NumberOfCustomers = config.NumberOfCustomers;
            _config.TimeDataWeeks = config.TimeDataWeeks;
            _config.StartDate = config.StartDate;
        }

        // Delete existing database file
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }

        await GenerateAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the path to the generated SQLite database file.
    /// </summary>
    public string GetDatabasePath() => _databasePath;

    /// <summary>
    /// Clears all data from the database.
    /// </summary>
    private async System.Threading.Tasks.Task ClearDatabaseAsync(LegatroDbContext context, CancellationToken cancellationToken)
    {
        // Delete in reverse dependency order
        await context.TimeItems.ExecuteDeleteAsync(cancellationToken);
        await context.Tasks.ExecuteDeleteAsync(cancellationToken);
        await context.Projects.ExecuteDeleteAsync(cancellationToken);
        await context.Customers.ExecuteDeleteAsync(cancellationToken);
        await context.Categories.Where(c => !c.IsSystemCategory).ExecuteDeleteAsync(cancellationToken);
        await context.Users.Where(u => !u.IsSystemAccount).ExecuteDeleteAsync(cancellationToken);
        await context.Contacts.ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Generates demo users with contacts.
    /// </summary>
    private async System.Threading.Tasks.Task<List<User>> GenerateUsersAsync(LegatroDbContext context, CancellationToken cancellationToken)
    {
        var users = new List<User>();
        var userTemplates = new[]
        {
            new { Username = "alice.dev", FirstName = "Alice", LastName = "Smith", Role = "Developer" },
            new { Username = "bob.designer", FirstName = "Bob", LastName = "Johnson", Role = "Designer" },
            new { Username = "carol.pm", FirstName = "Carol", LastName = "Williams", Role = "Project Manager" }
        };

        foreach (var template in userTemplates.Take(_config.NumberOfUsers))
        {
            var contact = new Contact
            {
                IdContact = Guid.NewGuid(),
                MainName = $"{template.FirstName} {template.LastName}",
                Email = $"{template.Username}@example.com",
                PhoneBusiness = "+1-555-0100",
                Address1 = "123 Main St",
                City = "San Francisco",
                Country = "USA",
                Zip = "94102",
                DateCreated = DateTime.UtcNow,
                DateLastEdited = DateTime.UtcNow,
                SyncGuid = Guid.NewGuid()
            };

            var user = new User
            {
                IdUser = Guid.NewGuid(),
                IdContact = contact.IdContact,
                Contact = contact,
                Username = template.Username,
                FirstName = template.FirstName,
                LastName = template.LastName,
                IsActivated = true,
                IsAdmin = false,
                IsSystemAccount = false,
                ClearanceLevel = 100,
                DateCreated = DateTime.UtcNow,
                DateLastEdited = DateTime.UtcNow,
                SyncGuid = Guid.NewGuid()
            };

            users.Add(user);
            context.Contacts.Add(contact);
            context.Users.Add(user);
        }

        await context.SaveChangesAsync(cancellationToken);
        return users;
    }

    /// <summary>
    /// Generates demo customers with contacts.
    /// </summary>
    private async System.Threading.Tasks.Task<List<Customer>> GenerateCustomersAsync(LegatroDbContext context, CancellationToken cancellationToken)
    {
        var customers = new List<Customer>();
        var customerTemplates = new[]
        {
            new { Name = "Contoso Ltd.", Industry = "Technology" },
            new { Name = "Northwind Traders", Industry = "Retail" },
            new { Name = "Adventure Works", Industry = "Manufacturing" },
            new { Name = "Fabrikam Inc.", Industry = "Finance" },
            new { Name = "Tailspin Toys", Industry = "Entertainment" }
        };

        for (int i = 0; i < _config.NumberOfCustomers; i++)
        {
            var template = customerTemplates[i % customerTemplates.Length];

            var contact = new Contact
            {
                IdContact = Guid.NewGuid(),
                MainName = $"{template.Name} - Contact",
                Email = $"contact{i}@{template.Name.Replace(" ", "").ToLower()}.com",
                PhoneBusiness = "+1-555-0200",
                Address1 = $"{i + 1} Business Ave",
                City = "New York",
                Country = "USA",
                Zip = "10001",
                DateCreated = DateTime.UtcNow,
                DateLastEdited = DateTime.UtcNow,
                SyncGuid = Guid.NewGuid()
            };

            var customer = new Customer
            {
                IdCustomer = Guid.NewGuid(),
                IdCompanyContact = contact.IdContact,
                CompanyContact = contact,
                CompanyName = template.Name,
                CustomerNumber = i + 1,
                IsIndividual = false,
                IsActive = true,
                DateCreated = DateTime.UtcNow,
                DateLastEdited = DateTime.UtcNow,
                SyncGuid = Guid.NewGuid()
            };

            customers.Add(customer);
            context.Contacts.Add(contact);
            context.Customers.Add(customer);
        }

        await context.SaveChangesAsync(cancellationToken);
        return customers;
    }

    /// <summary>
    /// Generates demo projects and tasks.
    /// </summary>
    private async System.Threading.Tasks.Task GenerateProjectsAsync(
        LegatroDbContext context,
        List<User> users,
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var projectNames = new[]
        {
            "E-Commerce Platform", "Mobile App Backend", "API Gateway",
            "Corporate Website Redesign", "Landing Page", "Brand Portal",
            "Q1 Sprint Planning", "Client Onboarding", "Process Optimization"
        };

        var taskNames = new[]
        {
            "Implement authentication", "Database schema design", "Unit test coverage",
            "Code review", "Bug fixes", "Wireframe creation", "Color palette selection",
            "Responsive layout", "Icon design", "Accessibility audit", "Stakeholder meeting",
            "Risk assessment", "Timeline review", "Resource allocation", "Status reporting"
        };

        foreach (var user in users)
        {
            for (int p = 0; p < _config.ProjectsPerUser; p++)
            {
                var projectName = projectNames[(user.Username.Length + p) % projectNames.Length];
                var customer = customers[p % customers.Count];

                var project = new Project
                {
                    IdProject = Guid.NewGuid(),
                    IdUserAsOwner = user.IdUser,
                    Owner = user,
                    IdCustomer = customer.IdCustomer,
                    Customer = customer,
                    ProjectNumber = users.IndexOf(user) * _config.ProjectsPerUser + p + 1,
                    ProjectName = projectName,
                    ShortProjectName = projectName.Substring(0, Math.Min(30, projectName.Length)),
                    IsProject = true,
                    IsActive = true,
                    IsSubProject = false,
                    Description = $"Project for {customer.CompanyName}",
                    MonitorTimeCapacity = p % 2 == 0,
                    MonthlyTargetTimeCapacity = p % 2 == 0 ? 160 * 60 : null,
                    DateCreated = DateTime.UtcNow,
                    DateLastEdited = DateTime.UtcNow,
                    SyncGuid = Guid.NewGuid()
                };

                context.Projects.Add(project);

                // Generate tasks for this project
                for (int t = 0; t < _config.TasksPerProject; t++)
                {
                    var taskName = taskNames[(projectName.Length + t) % taskNames.Length];
                    var dueDate = _config.StartDate.AddDays(30 + t * 2);

                    var task = new LegatroTask
                    {
                        IdTask = Guid.NewGuid(),
                        IdProject = project.IdProject,
                        Project = project,
                        IdUserAsOwner = user.IdUser,
                        Owner = user,
                        TaskName = taskName,
                        TaskDescription = $"Task: {taskName} for {projectName}",
                        TaskOrderNo = t + 1,
                        DueDate = dueDate,
                        TaskDone = false,
                        PlanedCapacityInMinutes = 30 + t * 30, // 30-180 minutes
                        IsTemplate = false,
                        DateCreated = DateTime.UtcNow,
                        DateLastEdited = DateTime.UtcNow,
                        SyncGuid = Guid.NewGuid()
                    };

                    context.Tasks.Add(task);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Generates realistic time tracking data for the configured number of weeks.
    /// </summary>
    private async System.Threading.Tasks.Task GenerateTimeDataAsync(LegatroDbContext context, List<User> users, CancellationToken cancellationToken)
    {
        var projects = await context.Projects.Include(p => p.Tasks).ToListAsync(cancellationToken);
        var tasks = await context.Tasks.Include(t => t.Project).ToListAsync(cancellationToken);
        var checkInType = await context.TimeItemTypes.FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckIn, cancellationToken);
        var checkOutType = await context.TimeItemTypes.FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckOut, cancellationToken);
        var breakType = await context.TimeItemTypes.FirstAsync(t => t.BookingType == (short)TimeItemBookingType.Break, cancellationToken);
        var setBookingType = await context.TimeItemTypes.FirstAsync(t => t.BookingType == (short)TimeItemBookingType.SetBooking, cancellationToken);

        var random = new Random(42); // Fixed seed for reproducibility

        foreach (var user in users)
        {
            var userProjects = projects.Where(p => p.IdUserAsOwner == user.IdUser).ToList();

            for (int week = 0; week < _config.TimeDataWeeks; week++)
            {
                var weekStart = _config.StartDate.AddDays(week * 7);

                for (int day = 0; day < 5; day++) // Weekdays only
                {
                    var currentDate = weekStart.AddDays(day);

                    // Skip if user joined after this date (not applicable for demo)
                    // Generate daily work pattern

                    // 08:00 - CheckIn (with random variance ±30 minutes)
                    var checkInTime = currentDate.AddHours(8 + (random.NextDouble() - 0.5));
                    await CreateTimeItemAsync(context, user, checkInTime, checkInType, null, null, cancellationToken);

                    // 08:00-10:00 - Work on Task A (SetBooking)
                    var currentProject = userProjects[day % userProjects.Count];
                    var projectTasks = currentProject.Tasks.ToList();
                    var currentTask = projectTasks[0];
                    await CreateTimeItemAsync(context, user, checkInTime.AddMinutes(120), setBookingType, currentProject, currentTask, cancellationToken, $"Working on {currentTask.TaskName}");

                    // 10:00 - Break (15 min)
                    var break1Start = checkInTime.AddMinutes(120);
                    await CreateTimeItemAsync(context, user, break1Start, breakType, null, null, cancellationToken);

                    // 10:15 - CheckIn (resume)
                    var resume1Start = break1Start.AddMinutes(15);
                    await CreateTimeItemAsync(context, user, resume1Start, checkInType, null, null, cancellationToken, "Resume work");

                    // 10:15-12:00 - Work on Task B
                    currentTask = projectTasks[1 % projectTasks.Count];
                    await CreateTimeItemAsync(context, user, resume1Start.AddMinutes(105), setBookingType, currentProject, currentTask, cancellationToken, $"Working on {currentTask.TaskName}");

                    // 12:00 - Break (45 min lunch)
                    var lunchStart = resume1Start.AddMinutes(105);
                    await CreateTimeItemAsync(context, user, lunchStart, breakType, null, null, cancellationToken, "Lunch");

                    // 12:45 - CheckIn (resume)
                    var resume2Start = lunchStart.AddMinutes(45);
                    await CreateTimeItemAsync(context, user, resume2Start, checkInType, null, null, cancellationToken, "Resume work");

                    // 12:45-15:00 - Work on Task C
                    currentTask = projectTasks[2 % projectTasks.Count];
                    await CreateTimeItemAsync(context, user, resume2Start.AddMinutes(135), setBookingType, currentProject, currentTask, cancellationToken, $"Working on {currentTask.TaskName}");

                    // 15:00 - Break (10 min)
                    var break2Start = resume2Start.AddMinutes(135);
                    await CreateTimeItemAsync(context, user, break2Start, breakType, null, null, cancellationToken);

                    // 15:10 - CheckIn (resume)
                    var resume3Start = break2Start.AddMinutes(10);
                    await CreateTimeItemAsync(context, user, resume3Start, checkInType, null, null, cancellationToken, "Resume work");

                    // 15:10-17:00 - Work on Task D
                    currentTask = projectTasks[3 % projectTasks.Count];
                    await CreateTimeItemAsync(context, user, resume3Start.AddMinutes(110), setBookingType, currentProject, currentTask, cancellationToken, $"Working on {currentTask.TaskName}");

                    // 17:00 - CheckOut
                    var checkOutTime = resume3Start.AddMinutes(110);
                    await CreateTimeItemAsync(context, user, checkOutTime, checkOutType, null, null, cancellationToken);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a time item with the specified parameters.
    /// </summary>
    private async System.Threading.Tasks.Task CreateTimeItemAsync(
        LegatroDbContext context,
        User user,
        DateTime eventTime,
        Legatro.DataLayer.Entities.TimeItemType timeItemType,
        Project? project,
        LegatroTask? task,
        CancellationToken cancellationToken,
        string? description = null)
    {
        var timeItem = new TimeItem
        {
            IdTimeItem = Guid.NewGuid(),
            IdUser = user.IdUser,
            User = user,
            IdProject = project?.IdProject,
            Project = project,
            IdTask = task?.IdTask,
            Task = task,
            IdTimeItemType = timeItemType.IdTimeItemType,
            TimeItemType = timeItemType,
            EventType = EventType.Time,
            ShortTitel = description ?? timeItemType.ShortName,
            EventTime = eventTime,
            BookingDateGMT = eventTime.Date,
            IsCompleted = false,
            IsDeleted = false,
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };

        context.TimeItems.Add(timeItem);
    }
}

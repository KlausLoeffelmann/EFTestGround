using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Services.Interfaces;

namespace Legatro.DataLayer.DemoData;

/// <summary>
/// Generates a realistic SQLite test database for development and testing.
/// </summary>
public class LegatroDemoDatabaseGenerator : IDemoDatabaseGenerator
{
    private readonly string _databasePath;
    private readonly Func<string, LegatroDbContext> _contextFactory;
    private Random _random = new(42);

    // Demo data templates
    private static readonly (string Username, string FirstName, string LastName, string Role)[] DemoUsers =
    {
        ("alice.dev", "Alice", "Developer", "Senior .NET Developer"),
        ("bob.designer", "Bob", "Designer", "UI/UX Designer"),
        ("carol.pm", "Carol", "Manager", "Project Manager")
    };

    private static readonly (string Name, string ShortName, string[] Tasks)[] SoftwareProjects =
    {
        ("E-Commerce Platform", "EComm", new[] { "Implement authentication", "Database schema design", "Unit test coverage", "Code review", "API development" }),
        ("Mobile App Backend", "MobileAPI", new[] { "RESTful API design", "Push notifications", "User management", "Data synchronization", "Performance optimization" }),
        ("API Gateway", "Gateway", new[] { "Rate limiting", "Authentication middleware", "Logging implementation", "Error handling", "Documentation" })
    };

    private static readonly (string Name, string ShortName, string[] Tasks)[] DesignProjects =
    {
        ("Corporate Website Redesign", "CorpWeb", new[] { "Wireframe creation", "Color palette selection", "Responsive layout", "Icon design", "Accessibility audit" }),
        ("Landing Page", "Landing", new[] { "Hero section design", "CTA optimization", "A/B testing", "Mobile optimization", "Animation effects" }),
        ("Brand Portal", "BrandPort", new[] { "Style guide creation", "Asset library", "Template design", "Component library", "Documentation" })
    };

    private static readonly (string Name, string ShortName, string[] Tasks)[] ManagementProjects =
    {
        ("Q1 Sprint Planning", "Q1Sprint", new[] { "Stakeholder meeting", "Risk assessment", "Timeline review", "Resource allocation", "Status reporting" }),
        ("Client Onboarding", "Onboard", new[] { "Requirements gathering", "Contract review", "Kickoff meeting", "Setup checklist", "Training plan" }),
        ("Process Optimization", "ProcOpt", new[] { "Current state analysis", "Gap identification", "Solution design", "Implementation plan", "Metrics definition" })
    };

    private static readonly (string CompanyName, string Industry)[] DemoCustomers =
    {
        ("Contoso Ltd.", "Technology"),
        ("Northwind Traders", "Retail"),
        ("Adventure Works", "Manufacturing"),
        ("Fabrikam Inc.", "Finance"),
        ("Tailspin Toys", "Entertainment")
    };

    public LegatroDemoDatabaseGenerator(string databasePath = "legatro-demo.db")
    {
        _databasePath = databasePath;
        _contextFactory = connectionString =>
            LegatroDbContextFactory.Create(DatabaseProviderType.Sqlite, connectionString);
    }

    public LegatroDemoDatabaseGenerator(Func<string, LegatroDbContext> contextFactory, string databasePath = "legatro-demo.db")
    {
        _databasePath = databasePath;
        _contextFactory = contextFactory;
    }

    public string GetDatabasePath() => _databasePath;

    public async Task GenerateAsync(DemoDataConfiguration config, CancellationToken ct = default)
    {
        _random = config.RandomSeed.HasValue ? new Random(config.RandomSeed.Value) : new Random();

        var connectionString = $"Data Source={_databasePath}";
        await using var context = _contextFactory(connectionString);

        // Ensure database is created
        await context.Database.EnsureCreatedAsync(ct);

        // Check if data already exists
        if (await context.Users.AnyAsync(ct))
        {
            return; // Data already exists
        }

        // Generate data
        var users = await GenerateUsersAsync(context, config.NumberOfUsers, ct);
        var customers = await GenerateCustomersAsync(context, config.NumberOfCustomers, ct);
        var projects = await GenerateProjectsAsync(context, users, customers, config.ProjectsPerUser, ct);
        await GenerateTasksAsync(context, projects, config.TasksPerProject, ct);
        await GenerateTimeDataAsync(context, users, projects, config, ct);

        await context.SaveChangesAsync(ct);
    }

    public async Task RegenerateAsync(DemoDataConfiguration config, CancellationToken ct = default)
    {
        var connectionString = $"Data Source={_databasePath}";
        await using var context = _contextFactory(connectionString);

        // Drop and recreate database
        await context.Database.EnsureDeletedAsync(ct);
        await GenerateAsync(config, ct);
    }

    private async Task<List<User>> GenerateUsersAsync(LegatroDbContext context, int count, CancellationToken ct)
    {
        var users = new List<User>();

        for (var i = 0; i < Math.Min(count, DemoUsers.Length); i++)
        {
            var template = DemoUsers[i];

            // Create contact
            var contact = new Contact
            {
                IdContact = Guid.NewGuid(),
                MainName = $"{template.FirstName} {template.LastName}",
                Email = $"{template.Username}@legatro.demo",
                PhoneMobile = $"+1-555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Address1 = $"{_random.Next(100, 9999)} Demo Street",
                City = "Demo City",
                Zip = $"{_random.Next(10000, 99999)}",
                Country = "Germany"
            };
            context.Contacts.Add(contact);

            // Create user
            var user = new User
            {
                IdUser = Guid.NewGuid(),
                IdContact = contact.IdContact,
                Username = template.Username,
                FirstName = template.FirstName,
                LastName = template.LastName,
                IsActivated = true,
                ClearanceLevel = i == 2 ? 100 : 50, // PM gets higher clearance
                IsAdmin = i == 2,
                DateOfJoining = DateTime.UtcNow.AddYears(-_random.Next(1, 5))
            };
            context.Users.Add(user);
            users.Add(user);
        }

        await context.SaveChangesAsync(ct);
        return users;
    }

    private async Task<List<Customer>> GenerateCustomersAsync(LegatroDbContext context, int count, CancellationToken ct)
    {
        var customers = new List<Customer>();

        for (var i = 0; i < Math.Min(count, DemoCustomers.Length); i++)
        {
            var template = DemoCustomers[i];

            // Create contact
            var contact = new Contact
            {
                IdContact = Guid.NewGuid(),
                MainName = template.CompanyName,
                Email = $"info@{template.CompanyName.ToLower().Replace(" ", "").Replace(".", "")}.demo",
                PhoneBusiness = $"+1-555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Address1 = $"{_random.Next(1, 999)} Business Ave",
                City = "Business City",
                Zip = $"{_random.Next(10000, 99999)}",
                Country = "Germany"
            };
            context.Contacts.Add(contact);

            // Create customer
            var customer = new Customer
            {
                IdCustomer = Guid.NewGuid(),
                IdCompanyContact = contact.IdContact,
                CustomerNumber = 1000 + i,
                CompanyName = template.CompanyName,
                IsActive = true,
                IsIndividual = false
            };
            context.Customers.Add(customer);
            customers.Add(customer);
        }

        await context.SaveChangesAsync(ct);
        return customers;
    }

    private async Task<List<Project>> GenerateProjectsAsync(
        LegatroDbContext context,
        List<User> users,
        List<Customer> customers,
        int projectsPerUser,
        CancellationToken ct)
    {
        var projects = new List<Project>();
        var projectNumber = 1;

        var projectTemplates = new[]
        {
            SoftwareProjects,
            DesignProjects,
            ManagementProjects
        };

        for (var userIndex = 0; userIndex < users.Count; userIndex++)
        {
            var user = users[userIndex];
            var templates = projectTemplates[userIndex % projectTemplates.Length];

            for (var i = 0; i < Math.Min(projectsPerUser, templates.Length); i++)
            {
                var template = templates[i];
                var customer = customers[_random.Next(customers.Count)];

                var project = new Project
                {
                    IdProject = Guid.NewGuid(),
                    IdUserAsOwner = user.IdUser,
                    IdCustomer = customer.IdCustomer,
                    ProjectNumber = projectNumber++,
                    ProjectName = template.Name,
                    ShortProjectName = template.ShortName,
                    IsProject = true,
                    IsActive = true,
                    MonitorTimeCapacity = true,
                    MonthlyTargetTimeCapacity = _random.Next(2000, 8000) // 33-133 hours
                };
                context.Projects.Add(project);
                projects.Add(project);
            }
        }

        await context.SaveChangesAsync(ct);
        return projects;
    }

    private async Task GenerateTasksAsync(
        LegatroDbContext context,
        List<Project> projects,
        int tasksPerProject,
        CancellationToken ct)
    {
        var projectTemplates = new Dictionary<string, string[]>();
        foreach (var p in SoftwareProjects) projectTemplates[p.Name] = p.Tasks;
        foreach (var p in DesignProjects) projectTemplates[p.Name] = p.Tasks;
        foreach (var p in ManagementProjects) projectTemplates[p.Name] = p.Tasks;

        foreach (var project in projects)
        {
            var taskTemplates = projectTemplates.GetValueOrDefault(project.ProjectName) ??
                new[] { "Task 1", "Task 2", "Task 3", "Task 4", "Task 5" };

            for (var i = 0; i < Math.Min(tasksPerProject, taskTemplates.Length); i++)
            {
                var task = new LegatroTask
                {
                    IdTask = Guid.NewGuid(),
                    IdProject = project.IdProject,
                    IdUserAsOwner = project.IdUserAsOwner,
                    TaskName = taskTemplates[i],
                    TaskDescription = $"Description for {taskTemplates[i]}",
                    TaskNo = i + 1,
                    TaskOrderNo = i + 1,
                    DueDate = DateTime.UtcNow.AddDays(_random.Next(7, 60)),
                    PlanedCapacityInMinutes = _random.Next(30, 480)
                };
                context.Tasks.Add(task);
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task GenerateTimeDataAsync(
        LegatroDbContext context,
        List<User> users,
        List<Project> projects,
        DemoDataConfiguration config,
        CancellationToken ct)
    {
        var endDate = config.StartDate.AddDays(config.TimeDataWeeks * 7);
        var userProjects = projects.GroupBy(p => p.IdUserAsOwner).ToDictionary(g => g.Key!.Value, g => g.ToList());

        foreach (var user in users)
        {
            var userProjectList = userProjects.GetValueOrDefault(user.IdUser) ?? new List<Project>();
            if (!userProjectList.Any()) continue;

            for (var date = config.StartDate.Date; date < endDate; date = date.AddDays(1))
            {
                // Skip weekends
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                await GenerateDayTimeItemsAsync(context, user, date, userProjectList, ct);
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task GenerateDayTimeItemsAsync(
        LegatroDbContext context,
        User user,
        DateTime date,
        List<Project> userProjects,
        CancellationToken ct)
    {
        var baseDate = date.Date;
        var items = new List<Entities.TimeItem>();

        // Variance in minutes
        var startVariance = TimeSpan.FromMinutes(_random.Next(-30, 30));
        var endVariance = TimeSpan.FromMinutes(_random.Next(-60, 60));

        // Morning check-in (around 8:00)
        var checkInTime = new DateTimeOffset(baseDate.AddHours(8).Add(startVariance), TimeSpan.Zero);
        items.Add(CreateTimeItem(user.IdUser, date, checkInTime, TimeItemTypeIds.CheckIn, "Check In"));

        // Morning work session
        var project1 = userProjects[_random.Next(userProjects.Count)];
        items.Add(CreateTimeItem(user.IdUser, date, checkInTime.AddSeconds(1), TimeItemTypeIds.SetBooking,
            $"Working on {project1.ShortProjectName}", project1.IdProject));

        // Morning break (around 10:00)
        var breakTime1 = checkInTime.AddHours(2);
        items.Add(CreateTimeItem(user.IdUser, date, breakTime1, TimeItemTypeIds.Break, "Coffee break"));
        items.Add(CreateTimeItem(user.IdUser, date, breakTime1.AddMinutes(15), TimeItemTypeIds.CheckIn, "Resume work"));

        // Late morning work
        var project2 = userProjects[_random.Next(userProjects.Count)];
        items.Add(CreateTimeItem(user.IdUser, date, breakTime1.AddMinutes(16), TimeItemTypeIds.SetBooking,
            $"Working on {project2.ShortProjectName}", project2.IdProject));

        // Lunch break (around 12:00)
        var lunchTime = checkInTime.AddHours(4);
        items.Add(CreateTimeItem(user.IdUser, date, lunchTime, TimeItemTypeIds.Break, "Lunch"));
        items.Add(CreateTimeItem(user.IdUser, date, lunchTime.AddMinutes(45), TimeItemTypeIds.CheckIn, "Resume work"));

        // Afternoon work
        var project3 = userProjects[_random.Next(userProjects.Count)];
        items.Add(CreateTimeItem(user.IdUser, date, lunchTime.AddMinutes(46), TimeItemTypeIds.SetBooking,
            $"Working on {project3.ShortProjectName}", project3.IdProject));

        // Afternoon break (around 15:00)
        var breakTime2 = lunchTime.AddHours(3);
        items.Add(CreateTimeItem(user.IdUser, date, breakTime2, TimeItemTypeIds.Break, "Short break"));
        items.Add(CreateTimeItem(user.IdUser, date, breakTime2.AddMinutes(10), TimeItemTypeIds.CheckIn, "Resume work"));

        // Late afternoon work
        items.Add(CreateTimeItem(user.IdUser, date, breakTime2.AddMinutes(11), TimeItemTypeIds.SetBooking,
            $"Continuing {project3.ShortProjectName}", project3.IdProject));

        // Check-out (around 17:00)
        var checkOutTime = checkInTime.AddHours(9).Add(endVariance);
        items.Add(CreateTimeItem(user.IdUser, date, checkOutTime, TimeItemTypeIds.CheckOut, "Check Out"));

        // Link items and add to context
        TimeItem? previousItem = null;
        foreach (var item in items.OrderBy(i => i.EventTime))
        {
            if (previousItem != null)
            {
                item.IdPreviousItem = previousItem.IdTimeItem;
                previousItem.IdNextItem = item.IdTimeItem;

                var duration = item.EventTime!.Value - previousItem.EventTime!.Value;
                previousItem.DurationToNext = duration;
                previousItem.DurationTicksToNext = duration.Ticks;
                item.DurationToPrevious = duration;
                item.DurationTicksToPrevious = duration.Ticks;
            }

            context.TimeItems.Add(item);
            previousItem = item;
        }
    }

    private static Entities.TimeItem CreateTimeItem(
        Guid userId,
        DateTime bookingDate,
        DateTimeOffset eventTime,
        Guid timeItemTypeId,
        string title,
        Guid? projectId = null)
    {
        return new Entities.TimeItem
        {
            IdTimeItem = Guid.NewGuid(),
            IdUser = userId,
            IdProject = projectId,
            IdTimeItemType = timeItemTypeId,
            EventType = EventType.Time,
            EventTime = eventTime,
            BookingDateGMT = bookingDate.Date,
            ShortTitel = title,
            Priority = 1000,
            SortOrder = eventTime.Ticks
        };
    }
}

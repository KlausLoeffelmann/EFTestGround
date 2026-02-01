# Database Layer Creation Prompt

## Overview

Create an EF Core data layer for a time tracking and project management application called **Legatro**. The database schema is defined in [`InitialDbSchema.md`](./InitialDbSchema.md).

---

## Requirements

### 1. Database Provider Support

The data layer must support multiple database providers:

- **SQLite** – for local development, testing, and lightweight deployments
- **SQL Server LocalDB** – for Windows development environments
- **SQL Server** – for production deployments

**Implementation Guidelines:**

- Use a `DbContextFactory` pattern or provider abstraction to switch between providers
- Configuration should be driven by connection strings and a provider enum/setting
- Handle provider-specific differences (e.g., `DbGeography` may require spatial extensions)
- Use EF Core migrations that are compatible with all target providers where possible
- Consider using `UseLazyLoadingProxies()` carefully; prefer explicit loading or projection

---

### 2. Entity Framework Core Configuration

- Use **Code-First** approach with Fluent API configuration
- Place entity configurations in separate `IEntityTypeConfiguration<T>` classes
- Apply indexes, constraints, and relationships as defined in the schema
- Use appropriate value converters for:
  - `TimeSpan` properties (store as ticks for SQL aggregation compatibility)
  - `Enum` types (store as underlying type)
  - `DateTimeOffset` (ensure consistent storage across providers)

---

### 3. Base Entity and Auditing

Create a base entity class that all entities inherit from:

```csharp
public abstract class BaseEntity
{
    public Guid SyncGuid { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateLastEdited { get; set; }
}
```

**Auditing Rules (applied automatically via `SaveChanges` override):**

- On **Add**: Set `DateCreated`, `DateLastEdited` to `DateTime.UtcNow`; generate new `SyncGuid`
- On **Modify**: Update `DateLastEdited` to `DateTime.UtcNow`; regenerate `SyncGuid`
- On **Delete** (soft delete): Set `IsDeleted` but do not delete physically (see exception for TimeItem table)

---

### 4. Seed Data / Base Data Generation

Create a seeding mechanism for:

- **TimeItemType** – All default system types as defined in the schema (Default, CheckIn, CheckOut, Break, Downtime, BusinessErrand, SetBooking)
- **System User** – A system account for automated operations
- **Default Categories** – If any system categories are required

Implement via:
- `HasData()` in Fluent API for static seed data, OR
- A `IDatabaseSeeder` service for dynamic/conditional seeding

---

### 5. TimeItem Business Logic (Critical)

The `TimeItem` table is the core of the time tracking system. It records **events**, not time ranges. This requires special handling.

#### 5.1 Event-Based Recording Model

- Each `TimeItem` represents a single point-in-time event
- Duration is calculated as the delta to the next/previous event
- Events are linked in a doubly-linked list per user per booking date

#### 5.2 Booking Plausibility Rules

Implement a `TimeItemValidator` or similar service that enforces these rules:

| Current State (Last Event) | Allowed Next Events | Auto-Insert Required |
|---------------------------|---------------------|----------------------|
| No events today           | CheckIn             | —                    |
| No events today           | Break, Downtime, BusinessErrand, SetBooking | Insert implicit CheckIn first |
| No events today           | CheckOut            | ❌ Throw exception |
| CheckIn                   | CheckOut, Break, Downtime, BusinessErrand, SetBooking | — |
| CheckOut                  | CheckIn             | — |
| CheckOut                  | CheckOut, Break, Downtime | ❌ Throw exception |
| Break                     | CheckIn (resume), CheckOut | — |
| Downtime                  | CheckIn (resume), CheckOut | — |

**Validation must:**
- Query the last event for the user on the same `BookingDateGMT`
- Evaluate the `TimeItemBookingType` of the new event against the last event
- Either reject with a descriptive exception, or auto-insert prerequisite events
- All auto-insertions must be part of the same transaction

#### 5.3 Linked List Maintenance

When a `TimeItem` is **inserted**:

1. Find the previous and next items for the same user and `BookingDateGMT` based on `EventTime`
2. Update `IdPreviousItem` and `IdNextItem` on the new item
3. Update `IdNextItem` on the previous item to point to the new item
4. Update `IdPreviousItem` on the next item to point to the new item
5. Recalculate `DurationToNext`/`DurationTicksToNext` on the previous item
6. Recalculate `DurationToPrevious`/`DurationTicksToPrevious` on the new item
7. Recalculate `DurationToNext`/`DurationTicksToNext` on the new item
8. Recalculate `DurationToPrevious`/`DurationTicksToPrevious` on the next item

When a `TimeItem` is **deleted** (soft delete):

1. Find the previous and next items
2. Link previous directly to next (bypass the deleted item)
3. Recalculate deltas between the newly adjacent items
4. Set `IsDeleted = true` on the deleted item

#### 5.4 Versioning / Change History

**TimeItems are never directly modified.** Changes create a versioned history:

1. **Copy** the entire existing `TimeItem` record
2. **Set** `DateValidTo` on the copy to `DateTime.UtcNow`
3. **Link** the copy via `IdHistoryParent` to preserve lineage
4. The original record ID remains; the copy becomes the archived version
5. **Update** the current record with the new values
6. **Recalculate** linked list pointers and deltas if `EventTime` changed

**Transaction Requirement:** All versioning operations must be atomic.

```csharp
public interface ITimeItemService
{
    Task<TimeItem> CreateAsync(TimeItem item, CancellationToken ct = default);
    Task<TimeItem> UpdateAsync(Guid id, Action<TimeItem> updateAction, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TimeItem>> GetHistoryAsync(Guid id, CancellationToken ct = default);
}
```

---

### 6. Domain-Specific Functionality by Entity

Each entity has implied business logic based on its fields. Implement appropriate services/repositories.

#### TimeItem
- **Purpose:** Core time tracking event store
- **Key Operations:** Event recording, linked-list traversal, duration aggregation, history tracking
- **Special Considerations:** Plausibility validation, auto-chaining, versioning
- **See:** [TimeItem Schema](./InitialDbSchema.md#entity-timeitem)

#### User
- **Purpose:** Employee/user identity and authentication
- **Key Operations:** Shadow employee management, activation/deactivation, clearance levels
- **Special Considerations:** `IsShadowEmployee` allows booking on behalf of others; `IdUserForShadow` links shadow to real user
- **See:** [User Schema](./InitialDbSchema.md#entity-user)

#### Task
- **Purpose:** Work items that can be tracked against time
- **Key Operations:** Hierarchical task management (parent/child), capacity planning, progress tracking
- **Special Considerations:** Self-referencing hierarchy via `IdParentTask`; templates via `IsTemplate`
- **See:** [Task Schema](./InitialDbSchema.md#entity-task)

#### Project
- **Purpose:** Container for tasks and time tracking target
- **Key Operations:** Project hierarchy, capacity monitoring, customer association
- **Special Considerations:** `MonitorTimeCapacity` enables budget tracking; `MaxBookedEmployees` limits concurrent workers
- **See:** [Project Schema](./InitialDbSchema.md#entity-project)

#### Customer
- **Purpose:** External clients for project billing and association
- **Key Operations:** Customer management, contact linking, activation status
- **Special Considerations:** `IsIndividual` distinguishes B2C from B2B; linked to `Contact` for address/phone
- **See:** [Customer Schema](./InitialDbSchema.md#entity-customer)

#### Vendor
- **Purpose:** Suppliers and external service providers
- **Key Operations:** Vendor management, product association
- **Special Considerations:** Structure mirrors Customer; linked to products via `IdVendor`
- **See:** [Vendor Schema](./InitialDbSchema.md#entity-vendor)

#### Category
- **Purpose:** Classification for TimeItems and Tasks
- **Key Operations:** Category CRUD, system category protection
- **Special Considerations:** `IsSystemCategory` prevents deletion of built-in categories
- **See:** [Category Schema](./InitialDbSchema.md#entity-category)

#### Product
- **Purpose:** Inventory and production tracking
- **Key Operations:** Stock management, pricing, vendor association
- **Special Considerations:** `TeHMin` = time units per production; `ReturningProductService` for service items
- **See:** [Product Schema](./InitialDbSchema.md#entity-product)

#### Contact
- **Purpose:** Address and contact information (shared by User, Customer, Vendor)
- **Key Operations:** Contact management, parent-child relationships
- **Special Considerations:** Self-referencing via `IdParentContact` for organizational hierarchy
- **See:** [Contact Schema](./InitialDbSchema.md#entity-contact)

#### TimeItemType
- **Purpose:** Defines booking event types and their behavior
- **Key Operations:** Type lookup, booking validation
- **Special Considerations:** `IsSystemType` prevents deletion; `BookingType` enum drives plausibility rules
- **See:** [TimeItemType Schema](./InitialDbSchema.md#entity-timeitemtype)

---

### 7. Repository / Service Layer Pattern

Implement a layered architecture:

```
Legatro.DataLayer/
├── Entities/                    # Entity classes
├── Configuration/               # IEntityTypeConfiguration<T> classes
├── Context/
│   ├── LegatroDbContext.cs
│   └── LegatroDbContextFactory.cs
├── Enums/                       # EventType, TimeItemBookingType, etc.
├── Services/
│   ├── ITimeItemService.cs
│   ├── TimeItemService.cs
│   ├── IUserService.cs
│   └── ...
├── Validators/
│   └── TimeItemValidator.cs
├── Seeding/
│   └── DatabaseSeeder.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

### 8. Performance Considerations

- Use `AsNoTracking()` for read-only queries
- Implement batch operations for linked-list recalculations
- Index `BookingDateGMT` + `IdUser` for fast event chain lookups
- Consider compiled queries for hot paths (e.g., getting last event for user/date)
- Use `ExecuteUpdateAsync` / `ExecuteDeleteAsync` for bulk operations where appropriate

---

### 9. Testing Requirements

- Unit tests for `TimeItemValidator` plausibility rules
- Integration tests for linked-list maintenance
- Integration tests for versioning/history
- Tests must use SQLite in-memory or a test database

---

### 10. Test Database Generation (SQLite)

Create a `LegatroDemoDatabaseGenerator` class that generates a realistic SQLite test database for development and testing purposes.

#### 10.1 Configuration

```csharp
public class DemoDataConfiguration
{
    public int NumberOfUsers { get; set; } = 3;
    public int ProjectsPerUser { get; set; } = 3;
    public int TasksPerProject { get; set; } = 5;
    public int NumberOfCustomers { get; set; } = 5;
    public int TimeDataWeeks { get; set; } = 4; // Adjustable: 1 month default
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-28);
}
```

#### 10.2 Demo Users

Generate at least 3 users with distinct roles:

| Username        | Role              | Description                          |
|-----------------|-------------------|--------------------------------------|
| `alice.dev`     | Developer         | Senior .NET developer                |
| `bob.designer`  | Designer          | UI/UX and web designer               |
| `carol.pm`      | Project Manager   | Oversees multiple projects           |

Each user should have:
- A linked `Contact` with realistic address/phone data
- `IsActivated = true`
- Appropriate `ClearanceLevel`

#### 10.3 Demo Projects

Create 3 projects per user across these domains:

| Project Type        | Example Names                                             |
|---------------------|-----------------------------------------------------------|
| Software Development| "E-Commerce Platform", "Mobile App Backend", "API Gateway"|
| Web Design          | "Corporate Website Redesign", "Landing Page", "Brand Portal"|
| Project Management  | "Q1 Sprint Planning", "Client Onboarding", "Process Optimization"|

Each project should have:
- Realistic `ProjectNumber` sequence
- `IsActive = true`
- Optional `IdCustomer` linking to a customer
- `MonitorTimeCapacity` enabled for some projects with `MonthlyTargetTimeCapacity`

#### 10.4 Demo Tasks

Create 5 tasks per project with realistic task names:

| Domain              | Example Tasks                                              |
|---------------------|-----------------------------------------------------------|
| Software Development| "Implement authentication", "Database schema design", "Unit test coverage", "Code review", "Bug fixes" |
| Web Design          | "Wireframe creation", "Color palette selection", "Responsive layout", "Icon design", "Accessibility audit" |
| Project Management  | "Stakeholder meeting", "Risk assessment", "Timeline review", "Resource allocation", "Status reporting" |

Task properties:
- Hierarchical structure (some tasks as subtasks via `IdParentTask`)
- `DueDate` within the test period
- `PlanedCapacityInMinutes` with realistic estimates (30-480 minutes)

#### 10.5 Demo Customers

Generate 5 realistic B2B customers:

| Customer Name              | Industry           | Projects Purchased |
|---------------------------|--------------------|--------------------|
| "Contoso Ltd."            | Technology         | Software dev       |
| "Northwind Traders"       | Retail             | E-commerce         |
| "Adventure Works"         | Manufacturing      | Web design         |
| "Fabrikam Inc."           | Finance            | API integration    |
| "Tailspin Toys"           | Entertainment      | Mobile development |

Each customer should have:
- Linked `Contact` (company contact)
- `IsActive = true`
- Realistic `CustomerNumber` sequence

#### 10.6 Demo Time Data Generation

Generate plausible time tracking data for the configured number of weeks:

**Work Pattern per User per Day (weekdays only):**

```
08:00 - CheckIn
08:00-10:00 - Work on Task A (SetBooking to project/task)
10:00 - Break (15 min)
10:15 - CheckIn (resume)
10:15-12:00 - Work on Task B
12:00 - Break (45 min lunch)
12:45 - CheckIn (resume)
12:45-15:00 - Work on Task C
15:00 - Break (10 min)
15:10 - CheckIn (resume)
15:10-17:00 - Work on Task D
17:00 - CheckOut
```

**Variations to include:**
- Some days with overtime (until 18:00-19:00)
- Some days with short hours (leave at 15:00)
- Occasional `BusinessErrand` entries (client visits)
- Occasional `Downtime` entries (waiting for deployment, blocked)
- Random variance in start/end times (±30 minutes)
- Skip weekends

**TimeItem Generation Rules:**
- All plausibility rules from Section 5.2 must be followed
- Linked list must be properly maintained
- Deltas must be calculated correctly
- Use realistic `ShortTitel` descriptions like "Working on user authentication module"

#### 10.7 Generator Interface

```csharp
public interface IDemoDatabaseGenerator
{
    /// <summary>
    /// Generates a complete demo database with the specified configuration.
    /// </summary>
    Task GenerateAsync(DemoDataConfiguration config, CancellationToken ct = default);

    /// <summary>
    /// Clears all data and regenerates the database.
    /// </summary>
    Task RegenerateAsync(DemoDataConfiguration config, CancellationToken ct = default);

    /// <summary>
    /// Returns the path to the generated SQLite database file.
    /// </summary>
    string GetDatabasePath();
}
```

#### 10.8 Test Fixture Integration

Create a test fixture that regenerates the database before tests run:

```csharp
public class LegatroTestFixture : IAsyncLifetime
{
    public LegatroDbContext Context { get; private set; } = null!;
    public IDemoDatabaseGenerator Generator { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Generator = new LegatroDemoDatabaseGenerator();
        await Generator.RegenerateAsync(new DemoDataConfiguration
        {
            TimeDataWeeks = 4
        });

        Context = CreateContext(Generator.GetDatabasePath());
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

---

### 11. TimeItem Unit Tests

Create comprehensive unit tests specifically for TimeItem table maintenance:

#### 11.1 Plausibility Validation Tests

```csharp
[Fact] FirstEventMustBeCheckIn_WhenNoEventsExist_ThrowsOnCheckOut()
[Fact] FirstEventMustBeCheckIn_WhenNoEventsExist_AutoInsertsCheckInBeforeBreak()
[Fact] CheckOut_AfterCheckIn_Succeeds()
[Fact] CheckOut_AfterCheckOut_ThrowsException()
[Fact] Break_AfterCheckIn_Succeeds()
[Fact] Downtime_WithNoEvents_AutoInsertsCheckInFirst()
[Fact] SetBooking_AfterCheckOut_ThrowsException()
```

#### 11.2 Linked List Maintenance Tests

```csharp
[Fact] Insert_FirstItem_HasNoPreviousOrNext()
[Fact] Insert_SecondItem_LinksToFirst()
[Fact] Insert_BetweenTwoItems_UpdatesBothLinks()
[Fact] Insert_UpdatesDurationToNext_OnPreviousItem()
[Fact] Insert_UpdatesDurationToPrevious_OnNextItem()
[Fact] Delete_MiddleItem_LinksPreviousToNext()
[Fact] Delete_RecalculatesDeltas_BetweenAdjacentItems()
[Fact] Delete_FirstItem_UpdatesNextItemPreviousToNull()
[Fact] Delete_LastItem_UpdatesPreviousItemNextToNull()
```

#### 11.3 Versioning Tests

```csharp
[Fact] Update_CreatesHistoryCopy_WithDateValidTo()
[Fact] Update_OriginalIdRemains_CopyGetsNewId()
[Fact] Update_HistoryCopy_LinkedViaIdHistoryParent()
[Fact] Update_RecalculatesLinkedList_WhenEventTimeChanges()
[Fact] GetHistory_ReturnsAllVersions_InChronologicalOrder()
[Fact] Update_IsAtomic_RollsBackOnFailure()
```

#### 11.4 Delta Calculation Tests

```csharp
[Fact] DurationToNext_CalculatedCorrectly_InTicksAndTimeSpan()
[Fact] DurationToPrevious_CalculatedCorrectly_InTicksAndTimeSpan()
[Fact] Insert_InMiddle_RecalculatesAllAffectedDeltas()
[Fact] Delete_RecalculatesDeltas_ForNewlyAdjacentItems()
```

---

### 12. Console Test Application

Create a console application (`LegatroTestConsole`) for interactive testing and demonstrations.

#### 12.1 Command Structure

Use a simple command parser with the following command categories:

```
legatro> help
Available commands:
  user     - User management commands
  project  - Project management commands
  task     - Task management commands
  category - Category management commands
  time     - Time tracking commands
  report   - Report generation commands
  db       - Database commands
  exit     - Exit the application
```

#### 12.2 User Commands

```
user list                          - List all users
user add <username> <first> <last> - Add a new user
user show <username>               - Show user details
user delete <username>             - Soft-delete a user
user activate <username>           - Activate a user
user deactivate <username>         - Deactivate a user
```

#### 12.3 Project Commands

```
project list                       - List all projects
project list --user <username>     - List projects for a user
project add <name> [--customer <id>] - Add a new project
project show <id|name>             - Show project details with tasks
project delete <id>                - Soft-delete a project
project assign <id> --user <username> - Assign user as owner
```

#### 12.4 Task Commands

```
task list                          - List all tasks
task list --project <id>           - List tasks for a project
task list --user <username>        - List tasks assigned to user
task add <name> --project <id> [--due <date>] [--capacity <minutes>]
task show <id>                     - Show task details
task complete <id>                 - Mark task as done
task delete <id>                   - Soft-delete a task
task parent <id> --parent <parentId> - Set parent task
```

#### 12.5 Category Commands

```
category list                      - List all categories
category add <name> [--desc <description>]
category delete <id>               - Delete category (fails for system categories)
```

#### 12.6 Time Tracking Commands

```
time checkin [--user <username>] [--time <HH:mm>] [--date <yyyy-MM-dd>]
time checkout [--user <username>] [--time <HH:mm>]
time break [--user <username>] [--duration <minutes>]
time book <projectId> [--task <taskId>] [--user <username>] [--desc <description>]
time downtime [--user <username>] [--reason <text>]
time errand [--user <username>] [--desc <description>]

time list --user <username> --date <yyyy-MM-dd>    - List day's events
time list --user <username> --week                 - List current week
time delete <timeItemId>                           - Soft-delete time item
time edit <timeItemId> --time <HH:mm>              - Edit event time (creates version)
time history <timeItemId>                          - Show version history
```

#### 12.7 Report Commands

```
report daily --user <username> [--date <yyyy-MM-dd>]
  Shows: All time items for the day with durations, total hours worked

report weekly --user <username> [--week <yyyy-Www>]
  Shows: Daily breakdown with totals, weekly summary

report monthly --user <username> [--month <yyyy-MM>]
  Shows: Weekly breakdown, monthly total, comparison to capacity

report project <projectId> [--from <date>] [--to <date>]
  Shows: All users' time on project, task breakdown, total hours

report project-summary [--from <date>] [--to <date>]
  Shows: All projects with total hours, sorted by hours desc

report user-summary [--from <date>] [--to <date>]
  Shows: All users with total hours, avg daily hours, projects worked

report overtime --user <username> [--month <yyyy-MM>]
  Shows: Days with >8 hours, total overtime

report tasks --user <username> [--status <open|done|all>]
  Shows: Tasks with time spent vs planned capacity
```

#### 12.8 Database Commands

```
db info                            - Show database path and stats
db regenerate [--weeks <n>]        - Regenerate demo database
db backup <path>                   - Backup database to path
db clear                           - Clear all data (with confirmation)
```

#### 12.9 Sample Report Outputs

**Daily Report:**
```
legatro> report daily --user alice.dev --date 2024-01-15

Daily Time Report: alice.dev - Monday, January 15, 2024
═══════════════════════════════════════════════════════
08:02  CheckIn
08:02  → E-Commerce Platform / Implement authentication
       Duration: 1h 58m
10:00  Break
       Duration: 15m
10:15  CheckIn (resume)
10:15  → E-Commerce Platform / Database schema design
       Duration: 1h 45m
12:00  Break (Lunch)
       Duration: 45m
12:45  CheckIn (resume)
12:45  → Mobile App Backend / API endpoint design
       Duration: 2h 15m
15:00  Break
       Duration: 10m
15:10  CheckIn (resume)
15:10  → E-Commerce Platform / Code review
       Duration: 1h 50m
17:00  CheckOut
───────────────────────────────────────────────────────
Total Work Time:    7h 48m
Total Break Time:   1h 10m
Projects Worked:    2
Tasks Completed:    0
```

**Project Summary Report:**
```
legatro> report project-summary --from 2024-01-01 --to 2024-01-31

Project Summary: January 2024
═══════════════════════════════════════════════════════════════
Project                      | Hours  | Users | Tasks | Status
────────────────────────────────────────────────────────────────
E-Commerce Platform          | 156.5h |   2   |  12   | Active
Mobile App Backend           |  89.2h |   1   |   8   | Active
Corporate Website Redesign   |  72.0h |   1   |   6   | Active
API Gateway                  |  45.5h |   2   |   5   | Active
Q1 Sprint Planning           |  23.0h |   1   |   4   | Active
────────────────────────────────────────────────────────────────
Total                        | 386.2h |   3   |  35   |
```

#### 12.10 Console Application Architecture

```csharp
public interface ICommandHandler
{
    string CommandName { get; }
    string Description { get; }
    Task<int> ExecuteAsync(string[] args, CancellationToken ct = default);
    void PrintHelp();
}

public class ConsoleApplication
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<string, ICommandHandler> _handlers;

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.WriteLine("Legatro Test Console v1.0");
        Console.WriteLine("Type 'help' for available commands.\n");

        while (!ct.IsCancellationRequested)
        {
            Console.Write("legatro> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            var parts = ParseCommandLine(input);
            if (parts.Length == 0) continue;

            if (parts[0] == "exit") break;

            await ExecuteCommandAsync(parts, ct);
        }
    }
}
```

---

## Summary Checklist

- [ ] Multi-provider support (SQLite, SQL Server LocalDB, SQL Server)
- [ ] Entity configurations via Fluent API
- [ ] Base entity with auditing (SyncGuid, DateCreated, DateLastEdited)
- [ ] Automatic field updates on SaveChanges
- [ ] Seed data for TimeItemTypes and system entities
- [ ] TimeItem plausibility validation with auto-insert
- [ ] TimeItem linked-list maintenance
- [ ] TimeItem versioning with history preservation
- [ ] Domain services for each entity
- [ ] Transaction handling for complex operations
- [ ] Demo database generator with configurable data volume
- [ ] Realistic demo data (users, projects, tasks, customers, time entries)
- [ ] Test fixture that regenerates database before test runs
- [ ] Unit tests for TimeItem plausibility rules
- [ ] Unit tests for linked-list maintenance
- [ ] Unit tests for versioning/history
- [ ] Unit tests for delta calculations
- [ ] Console application with interactive commands
- [ ] User/Project/Task/Category CRUD commands
- [ ] Time tracking commands (checkin, checkout, break, book, etc.)
- [ ] Report commands (daily, weekly, monthly, project, user summary)
- [ ] Database management commands



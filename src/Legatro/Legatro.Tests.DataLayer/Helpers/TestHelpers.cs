using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Microsoft.EntityFrameworkCore;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.Tests.DataLayer.Helpers;

/// <summary>
/// Helper methods for unit tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a test TimeItem with default values.
    /// </summary>
    public static TimeItem CreateTestTimeItem(
        Guid id,
        Guid userId,
        DateTime eventTime,
        TimeItemBookingType bookingType = TimeItemBookingType.Default,
        Guid? projectId = null,
        Guid? taskId = null,
        string? description = null)
    {
        return new TimeItem
        {
            IdTimeItem = id,
            IdUser = userId,
            IdProject = projectId,
            IdTask = taskId,
            IdTimeItemType = Guid.NewGuid(), // Will be set by context
            EventType = EventType.Time,
            ShortTitel = description ?? "Test TimeItem",
            EventTime = eventTime,
            BookingDateGMT = eventTime.Date,
            IsCompleted = false,
            IsDeleted = false,
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };
    }

    /// <summary>
    /// Creates a test User.
    /// </summary>
    public static User CreateTestUser(
        Guid userId,
        Guid contactId,
        string username = "test.user",
        string firstName = "Test",
        string lastName = "User")
    {
        var contact = new Contact
        {
            IdContact = contactId,
            MainName = $"{firstName} {lastName}",
            Email = $"{username}@example.com",
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };

        return new User
        {
            IdUser = userId,
            IdContact = contactId,
            Contact = contact,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            IsActivated = true,
            IsAdmin = false,
            IsSystemAccount = false,
            ClearanceLevel = 100,
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };
    }

    /// <summary>
    /// Creates a test Project.
    /// </summary>
    public static Project CreateTestProject(
        Guid projectId,
        Guid ownerId,
        Guid customerId,
        string name = "Test Project")
    {
        return new Project
        {
            IdProject = projectId,
            IdUserAsOwner = ownerId,
            IdCustomer = customerId,
            ProjectNumber = 1,
            ProjectName = name,
            ShortProjectName = name.Substring(0, Math.Min(30, name.Length)),
            IsProject = true,
            IsActive = true,
            Description = "Test project description",
            MonitorTimeCapacity = true,
            MonthlyTargetTimeCapacity = 160 * 60, // 160 hours in minutes
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };
    }

    /// <summary>
    /// Creates a test Task.
    /// </summary>
    public static LegatroTask CreateTestTask(
        Guid taskId,
        Guid projectId,
        Guid ownerId,
        string name = "Test Task")
    {
        return new LegatroTask
        {
            IdTask = taskId,
            IdProject = projectId,
            IdUserAsOwner = ownerId,
            TaskName = name,
            TaskDescription = "Test task description",
            TaskOrderNo = 1,
            DueDate = DateTime.UtcNow.AddDays(7),
            TaskDone = false,
            PlanedCapacityInMinutes = 60,
            IsTemplate = false,
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };
    }

    /// <summary>
    /// Seeds a TimeItemType for the given booking type.
    /// </summary>
    public static async System.Threading.Tasks.Task<TimeItemType> SeedTimeItemTypeAsync(
        LegatroDbContext context,
        TimeItemBookingType bookingType,
        string name,
        string shortName)
    {
        var existing = await context.TimeItemTypes
            .FirstOrDefaultAsync(t => t.BookingType == (short)bookingType);

        if (existing != null)
        {
            return existing;
        }

        var timeItemType = new TimeItemType
        {
            IdTimeItemType = Guid.NewGuid(),
            BookingType = (short)bookingType,
            TimeItemTypeName = name,
            ShortName = shortName,
            IsSystemType = true,
            DateCreated = DateTime.UtcNow,
            DateLastEdited = DateTime.UtcNow,
            SyncGuid = Guid.NewGuid()
        };

        context.TimeItemTypes.Add(timeItemType);
        await context.SaveChangesAsync();

        return timeItemType;
    }

    /// <summary>
    /// Seeds all standard TimeItemTypes for testing.
    /// </summary>
    public static async System.Threading.Tasks.Task SeedAllTimeItemTypesAsync(LegatroDbContext context)
    {
        await SeedTimeItemTypeAsync(context, TimeItemBookingType.Default, "Default", "Default");
        await SeedTimeItemTypeAsync(context, TimeItemBookingType.CheckIn, "Check In", "CheckIn");
        await SeedTimeItemTypeAsync(context, TimeItemBookingType.CheckOut, "Check Out", "CheckOut");
        await SeedTimeItemTypeAsync(context, TimeItemBookingType.Break, "Break", "Break");
        await SeedTimeItemTypeAsync(context, TimeItemBookingType.Downtime, "Downtime", "Downtime");
        await SeedTimeItemTypeAsync(context, TimeItemBookingType.BusinessErrand, "Business Errand", "BusinessErrand");
        await SeedTimeItemTypeAsync(context, TimeItemBookingType.SetBooking, "Set Booking", "SetBooking");
    }

    /// <summary>
    /// Asserts that two TimeItems are linked correctly.
    /// </summary>
    public static void AssertLinkedItems(TimeItem first, TimeItem second)
    {
        Assert.NotNull(first);
        Assert.NotNull(second);

        // First should point to second as next
        Assert.Equal(second.IdTimeItem, first.IdNextItem);
        Assert.NotNull(first.DurationToNext);
        Assert.True(first.DurationToNext > TimeSpan.Zero);

        // Second should point to first as previous
        Assert.Equal(first.IdTimeItem, second.IdPreviousItem);
        Assert.NotNull(second.DurationToPrevious);
        Assert.True(second.DurationToPrevious > TimeSpan.Zero);
    }

    /// <summary>
    /// Asserts that two TimeItems are not linked.
    /// </summary>
    public static void AssertNotLinkedItems(TimeItem first, TimeItem second)
    {
        Assert.NotNull(first);
        Assert.NotNull(second);

        Assert.Null(first.IdNextItem);
        Assert.Null(first.DurationToNext);
        Assert.Null(first.DurationTicksToNext);

        Assert.Null(second.IdPreviousItem);
        Assert.Null(second.DurationToPrevious);
        Assert.Null(second.DurationTicksToPrevious);
    }

    /// <summary>
    /// Gets the total booked time for a user on a specific date.
    /// </summary>
    public static async System.Threading.Tasks.Task<TimeSpan> GetTotalBookedTimeAsync(
        LegatroDbContext context,
        Guid userId,
        DateTime date)
    {
        var timeItems = await context.TimeItems
            .Where(t => t.IdUser == userId
                && t.BookingDateGMT == date
                && !t.IsDeleted)
            .ToListAsync();

        long totalTicks = timeItems.Sum(t => t.DurationTicksToNext ?? 0);
        return TimeSpan.FromTicks(totalTicks);
    }

    /// <summary>
    /// Waits for a task to complete with a timeout.
    /// </summary>
    public static async System.Threading.Tasks.Task<T> WithTimeoutAsync<T>(System.Threading.Tasks.Task<T> task, TimeSpan timeout)
    {
        var completedTask = await System.Threading.Tasks.Task.WhenAny(task, System.Threading.Tasks.Task.Delay(timeout));

        if (completedTask == task)
        {
            return await task;
        }

        throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
    }

    /// <summary>
    /// Clears all TimeItems for a specific user.
    /// </summary>
    public static async System.Threading.Tasks.Task ClearUserTimeItemsAsync(LegatroDbContext context, Guid userId)
    {
        var timeItems = await context.TimeItems
            .Where(t => t.IdUser == userId)
            .ToListAsync();

        context.TimeItems.RemoveRange(timeItems);
        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Extension methods for test helpers.
/// </summary>
public static class TestHelperExtensions
{
    /// <summary>
    /// Converts ticks to TimeSpan.
    /// </summary>
    public static TimeSpan? Ticks(this long? ticks)
    {
        return ticks.HasValue ? TimeSpan.FromTicks(ticks.Value) : null;
    }

    /// <summary>
    /// Converts ticks to TimeSpan.
    /// </summary>
    public static TimeSpan Ticks(this long ticks)
    {
        return TimeSpan.FromTicks(ticks);
    }
}
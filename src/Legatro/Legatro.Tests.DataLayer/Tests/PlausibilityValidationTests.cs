using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Exceptions;
using Legatro.DataLayer.Services;
using Legatro.Tests.DataLayer.Fixtures;
using Legatro.Tests.DataLayer.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.Tests.DataLayer.Tests;

/// <summary>
/// Tests for TimeItem plausibility validation rules.
/// </summary>
[Collection("Legatro Tests")]
public class PlausibilityValidationTests
{
    private readonly LegatroTestFixture _fixture;

    public PlausibilityValidationTests(LegatroTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckIn_AsFirstEvent_Succeeds()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await TestHelpers.SeedAllTimeItemTypesAsync(context);

        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var user = TestHelpers.CreateTestUser(userId, contactId, "test.user", "Test", "User");
        context.Users.Add(user);
        context.Contacts.Add(user.Contact!);
        await context.SaveChangesAsync();

        var timeItem = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckIn);

        var checkInType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckIn);
        timeItem.IdTimeItemType = checkInType.IdTimeItemType;

        var validator = new Legatro.DataLayer.Validators.TimeItemValidator(context);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await validator.ValidateTimeItemInsertAsync(timeItem);
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckOut_AsFirstEvent_Fails()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await TestHelpers.SeedAllTimeItemTypesAsync(context);

        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var user = TestHelpers.CreateTestUser(userId, contactId, "test.user", "Test", "User");
        context.Users.Add(user);
        context.Contacts.Add(user.Contact!);
        await context.SaveChangesAsync();

        var timeItem = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckOut);

        var checkOutType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckOut);
        timeItem.IdTimeItemType = checkOutType.IdTimeItemType;

        var validator = new Legatro.DataLayer.Validators.TimeItemValidator(context);

        // Act & Assert
        await Assert.ThrowsAsync<TimeItemValidationException>(async () =>
        {
            await validator.ValidateTimeItemInsertAsync(timeItem);
        });
    }

    [Fact]
    public async System.Threading.Tasks.Task CheckOut_RequiresPriorCheckIn()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await TestHelpers.SeedAllTimeItemTypesAsync(context);

        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var user = TestHelpers.CreateTestUser(userId, contactId, "test.user", "Test", "User");
        context.Users.Add(user);
        context.Contacts.Add(user.Contact!);
        await context.SaveChangesAsync();

        var checkInType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckIn);
        var checkOutType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckOut);

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-8),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;
        context.TimeItems.Add(checkIn);
        await context.SaveChangesAsync();

        var checkOut = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckOut);
        checkOut.IdTimeItemType = checkOutType.IdTimeItemType;

        var validator = new Legatro.DataLayer.Validators.TimeItemValidator(context);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await validator.ValidateTimeItemInsertAsync(checkOut);
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async System.Threading.Tasks.Task Break_RequiresPriorCheckIn()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await TestHelpers.SeedAllTimeItemTypesAsync(context);

        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var user = TestHelpers.CreateTestUser(userId, contactId, "test.user", "Test", "User");
        context.Users.Add(user);
        context.Contacts.Add(user.Contact!);
        await context.SaveChangesAsync();

        var checkInType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckIn);
        var breakType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.Break);

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-2),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;
        context.TimeItems.Add(checkIn);
        await context.SaveChangesAsync();

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var validator = new Legatro.DataLayer.Validators.TimeItemValidator(context);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await validator.ValidateTimeItemInsertAsync(@break);
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async System.Threading.Tasks.Task BusinessErrand_RequiresPriorCheckIn()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await TestHelpers.SeedAllTimeItemTypesAsync(context);

        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var user = TestHelpers.CreateTestUser(userId, contactId, "test.user", "Test", "User");
        context.Users.Add(user);
        context.Contacts.Add(user.Contact!);
        await context.SaveChangesAsync();

        var checkInType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckIn);
        var errandType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.BusinessErrand);

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-4),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;
        context.TimeItems.Add(checkIn);
        await context.SaveChangesAsync();

        var errand = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.BusinessErrand);
        errand.IdTimeItemType = errandType.IdTimeItemType;

        var validator = new Legatro.DataLayer.Validators.TimeItemValidator(context);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await validator.ValidateTimeItemInsertAsync(errand);
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async System.Threading.Tasks.Task Downtime_RequiresPriorCheckIn()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await TestHelpers.SeedAllTimeItemTypesAsync(context);

        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var user = TestHelpers.CreateTestUser(userId, contactId, "test.user", "Test", "User");
        context.Users.Add(user);
        context.Contacts.Add(user.Contact!);
        await context.SaveChangesAsync();

        var checkInType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckIn);
        var downtimeType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.Downtime);

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;
        context.TimeItems.Add(checkIn);
        await context.SaveChangesAsync();

        var downtime = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.Downtime);
        downtime.IdTimeItemType = downtimeType.IdTimeItemType;

        var validator = new Legatro.DataLayer.Validators.TimeItemValidator(context);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await validator.ValidateTimeItemInsertAsync(downtime);
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async System.Threading.Tasks.Task SetBooking_WorksAfterCheckIn()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await TestHelpers.SeedAllTimeItemTypesAsync(context);

        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var user = TestHelpers.CreateTestUser(userId, contactId, "test.user", "Test", "User");
        var companyContactId = Guid.NewGuid();
        var companyContact = new Contact
        {
            IdContact = companyContactId,
            Salutation = "Company",
            MainName = "Test Customer",
            Email = "company@testcustomer.com"
        };
        var customer = new Customer
        {
            IdCustomer = customerId,
            IdCompanyContact = companyContactId,
            CompanyName = "Test Customer",
            IsActive = true
        };
        var project = TestHelpers.CreateTestProject(projectId, userId, customerId, "Test Project");
        var task = TestHelpers.CreateTestTask(taskId, projectId, userId, "Test Task");

        // Add Contacts first to avoid FK constraint issues
        context.Contacts.Add(companyContact);
        context.Contacts.Add(user.Contact!);
        context.Customers.Add(customer);
        context.Users.Add(user);
        context.Projects.Add(project);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var checkInType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckIn);
        var setBookingType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.SetBooking);

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddMinutes(-30),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;
        context.TimeItems.Add(checkIn);
        await context.SaveChangesAsync();

        var setBooking = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.SetBooking,
            projectId,
            taskId,
            "Working on test task");
        setBooking.IdTimeItemType = setBookingType.IdTimeItemType;

        var validator = new Legatro.DataLayer.Validators.TimeItemValidator(context);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await validator.ValidateTimeItemInsertAsync(setBooking);
        });

        // Assert
        Assert.Null(exception);
    }
}
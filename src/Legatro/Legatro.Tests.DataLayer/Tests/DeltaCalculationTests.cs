using Legatro.DataLayer.Context;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Services;
using Legatro.Tests.DataLayer.Fixtures;
using Legatro.Tests.DataLayer.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using LegatroTask = Legatro.DataLayer.Entities.Task;

namespace Legatro.Tests.DataLayer.Tests;

/// <summary>
/// Tests for TimeItem delta calculation and total booked time.
/// </summary>
[Collection("Legatro Tests")]
public class DeltaCalculationTests
{
    private readonly LegatroTestFixture _fixture;

    public DeltaCalculationTests(LegatroTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTotalBookedTime_CalculatesCorrectDuration()
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
        var checkOutType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckOut);

        var baseTime = DateTime.UtcNow.Date.AddHours(8);

        // CheckIn at 08:00, Break at 10:00, CheckOut at 17:00
        // Total booked time should be ~8 hours (minus break which is not counted)
        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            baseTime,
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            baseTime.AddHours(2),
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var checkOut = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            baseTime.AddHours(9),
            TimeItemBookingType.CheckOut);
        checkOut.IdTimeItemType = checkOutType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);
        await service.CreateAsync(checkOut);

        // Act
        var totalTime = await service.GetTotalBookedTimeAsync(userId, baseTime.Date);

        // Assert - Should be ~7 hours (9 hours from checkIn to checkOut, minus 2 hour break)
        Assert.True(totalTime.TotalHours >= 6.9);
        Assert.True(totalTime.TotalHours <= 7.1);
    }

    [Fact]
    public async System.Threading.Tasks.Task DurationTicksToNext_StoresCorrectValue()
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

        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow;

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            startTime,
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            endTime,
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        // Act
        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);

        // Assert
        var createdCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);
        var expectedTicks = (endTime - startTime).Ticks;

        Assert.NotNull(createdCheckIn.DurationTicksToNext);
        Assert.Equal(expectedTicks, createdCheckIn.DurationTicksToNext);
    }

    [Fact]
    public async System.Threading.Tasks.Task DurationToNext_ReflectsSameValueAsTicks()
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
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);

        // Act
        var createdCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);

        // Assert
        Assert.NotNull(createdCheckIn.DurationToNext);
        Assert.NotNull(createdCheckIn.DurationTicksToNext);
        Assert.Equal(createdCheckIn.DurationToNext.Value.Ticks, createdCheckIn.DurationTicksToNext);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTotalBookedTime_IgnoresSoftDeletedItems()
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
        var checkOutType = await context.TimeItemTypes
            .FirstAsync(t => t.BookingType == (short)TimeItemBookingType.CheckOut);

        var baseTime = DateTime.UtcNow.Date.AddHours(8);

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            baseTime,
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            baseTime.AddHours(2),
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var checkOut = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            baseTime.AddHours(4),
            TimeItemBookingType.CheckOut);
        checkOut.IdTimeItemType = checkOutType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);
        await service.CreateAsync(checkOut);

        // Delete the break item
        await service.DeleteAsync(@break.IdTimeItem);

        // Act
        var totalTime = await service.GetTotalBookedTimeAsync(userId, baseTime.Date);

        // Assert - Should be 4 hours (checkIn to checkOut directly, without the break in between)
        Assert.True(totalTime.TotalHours >= 3.9);
        Assert.True(totalTime.TotalHours <= 4.1);
    }
}

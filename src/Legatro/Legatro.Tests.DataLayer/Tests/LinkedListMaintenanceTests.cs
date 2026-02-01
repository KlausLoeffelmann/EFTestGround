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
/// Tests for TimeItem linked-list maintenance.
/// </summary>
[Collection("Legatro Tests")]
public class LinkedListMaintenanceTests
{
    private readonly LegatroTestFixture _fixture;

    public LinkedListMaintenanceTests(LegatroTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async System.Threading.Tasks.Task InsertTimeItem_SetsPreviousLink()
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

        // Act
        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);

        // Assert
        var createdCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);
        var createdBreak = await service.GetByIdAsync(@break.IdTimeItem);

        Assert.NotNull(createdBreak.IdPreviousItem);
        Assert.Equal(checkIn.IdTimeItem, createdBreak.IdPreviousItem);
        Assert.NotNull(createdBreak.DurationToPrevious);
        Assert.True(createdBreak.DurationToPrevious.Value > TimeSpan.Zero);
    }

    [Fact]
    public async System.Threading.Tasks.Task InsertTimeItem_UpdatesNextLinkOfPrevious()
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

        // Act
        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);

        // Assert
        var createdCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);

        Assert.NotNull(createdCheckIn.IdNextItem);
        Assert.Equal(@break.IdTimeItem, createdCheckIn.IdNextItem);
        Assert.NotNull(createdCheckIn.DurationToNext);
        Assert.True(createdCheckIn.DurationToNext.Value > TimeSpan.Zero);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTimeItem_RelinksChain()
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

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-2),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var checkOut = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckOut);
        checkOut.IdTimeItemType = checkOutType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        // Act
        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);
        await service.CreateAsync(checkOut);

        await service.DeleteAsync(@break.IdTimeItem);

        // Assert
        var updatedCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);
        var updatedCheckOut = await service.GetByIdAsync(checkOut.IdTimeItem);

        Assert.NotNull(updatedCheckIn.IdNextItem);
        Assert.Equal(checkOut.IdTimeItem, updatedCheckIn.IdNextItem);
        Assert.NotNull(updatedCheckOut.IdPreviousItem);
        Assert.Equal(checkIn.IdTimeItem, updatedCheckOut.IdPreviousItem);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetChain_ReturnsAllLinkedItems()
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

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-2),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var checkOut = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckOut);
        checkOut.IdTimeItemType = checkOutType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);
        await service.CreateAsync(checkOut);

        // Act
        var chain = await service.GetChainAsync(checkIn.IdTimeItem, ChainDirection.Forward);

        // Assert
        Assert.Equal(3, chain.Count);
        Assert.Equal(checkIn.IdTimeItem, chain[0].IdTimeItem);
        Assert.Equal(@break.IdTimeItem, chain[1].IdTimeItem);
        Assert.Equal(checkOut.IdTimeItem, chain[2].IdTimeItem);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateEventTime_RecalculatesDurations()
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

        var checkInTime = DateTime.UtcNow.AddHours(-1);
        var breakTime = DateTime.UtcNow;

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            checkInTime,
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            breakTime,
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);

        var originalDuration = checkIn.DurationToNext;

        // Act - Move break 30 minutes later
        @break.EventTime = @break.EventTime.Value.AddMinutes(30);
        await service.UpdateAsync(@break);

        // Assert
        var updatedCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);
        var updatedBreak = await service.GetByIdAsync(@break.IdTimeItem);

        Assert.True(updatedCheckIn.DurationToNext > originalDuration);
        Assert.True(updatedBreak.DurationToPrevious!.Value.TotalMinutes >= 89); // 60 + 30
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteMiddleItem_CalculatesNewDuration()
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

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-2),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var @break = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.Break);
        @break.IdTimeItemType = breakType.IdTimeItemType;

        var checkOut = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckOut);
        checkOut.IdTimeItemType = checkOutType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(checkIn);
        await service.CreateAsync(@break);
        await service.CreateAsync(checkOut);

        // Act
        await service.DeleteAsync(@break.IdTimeItem);

        // Assert
        var updatedCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);

        // After deleting the middle item, checkIn should now link directly to checkOut
        // Duration should be approximately 2 hours (from checkIn at -2h to checkOut at now)
        Assert.NotNull(updatedCheckIn.DurationToNext);
        Assert.True(updatedCheckIn.DurationToNext.Value.TotalHours >= 1.9);
    }

    [Fact]
    public async System.Threading.Tasks.Task FirstItemInChain_HasNoPreviousLink()
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

        var checkIn = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        // Act
        await service.CreateAsync(checkIn);

        // Assert
        var createdCheckIn = await service.GetByIdAsync(checkIn.IdTimeItem);

        Assert.Null(createdCheckIn.IdPreviousItem);
        Assert.Null(createdCheckIn.DurationToPrevious);
    }

    [Fact]
    public async System.Threading.Tasks.Task LastItemInChain_HasNoNextLink()
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
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn);
        checkIn.IdTimeItemType = checkInType.IdTimeItemType;

        var checkOut = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            TimeItemBookingType.CheckOut);
        checkOut.IdTimeItemType = checkOutType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(checkIn);
        await service.CreateAsync(checkOut);

        // Act
        var createdCheckOut = await service.GetByIdAsync(checkOut.IdTimeItem);

        // Assert
        Assert.Null(createdCheckOut.IdNextItem);
        Assert.Null(createdCheckOut.DurationToNext);
    }
}

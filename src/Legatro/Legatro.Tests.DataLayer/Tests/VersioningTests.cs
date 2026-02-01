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
/// Tests for TimeItem versioning and history tracking.
/// </summary>
[Collection("Legatro Tests")]
public class VersioningTests
{
    private readonly LegatroTestFixture _fixture;

    public VersioningTests(LegatroTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTimeItem_CreatesHistoricalVersion()
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

        var timeItem = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn,
            description: "Original description");
        timeItem.IdTimeItemType = checkInType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(timeItem);

        // Act - Update the time item
        timeItem.ShortTitel = "Updated description";
        await service.UpdateAsync(timeItem);

        // Assert
        var history = await service.GetHistoryAsync(timeItem.IdTimeItem);

        Assert.Equal(2, history.Count);
        Assert.Contains(history, h => h.ShortTitel == "Updated description");
        Assert.Contains(history, h => h.ShortTitel == "Original description");
    }

    [Fact]
    public async System.Threading.Tasks.Task HistoricalVersion_HasDateValidToSet()
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

        var timeItem = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn);
        timeItem.IdTimeItemType = checkInType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(timeItem);

        // Act
        timeItem.ShortTitel = "Updated";
        await service.UpdateAsync(timeItem);

        // Assert
        var history = await service.GetHistoryAsync(timeItem.IdTimeItem);
        var historicalVersion = history.FirstOrDefault(h => h.ShortTitel == "Test TimeItem");

        Assert.NotNull(historicalVersion);
        Assert.NotNull(historicalVersion.DateValidTo);
        Assert.True(historicalVersion.DateValidTo > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async System.Threading.Tasks.Task CurrentVersion_HasDateValidToNull()
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

        var timeItem = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn);
        timeItem.IdTimeItemType = checkInType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(timeItem);
        timeItem.ShortTitel = "Updated";
        await service.UpdateAsync(timeItem);

        // Act
        var currentVersion = await service.GetByIdAsync(timeItem.IdTimeItem);

        // Assert
        Assert.NotNull(currentVersion);
        Assert.Null(currentVersion.DateValidTo);
    }

    [Fact]
    public async System.Threading.Tasks.Task HistoricalVersion_MaintainsIdHistoryParent()
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

        var timeItem = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn);
        timeItem.IdTimeItemType = checkInType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(timeItem);
        timeItem.ShortTitel = "Updated";
        await service.UpdateAsync(timeItem);

        // Act
        var history = await service.GetHistoryAsync(timeItem.IdTimeItem);
        var historicalVersion = history.FirstOrDefault(h => h.IdTimeItem != timeItem.IdTimeItem);

        // Assert
        Assert.NotNull(historicalVersion);
        Assert.Equal(timeItem.IdTimeItem, historicalVersion.IdHistoryParent);
    }

    [Fact]
    public async System.Threading.Tasks.Task MultipleUpdates_CreateMultipleHistoricalVersions()
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

        var timeItem = TestHelpers.CreateTestTimeItem(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddHours(-1),
            TimeItemBookingType.CheckIn,
            description: "Version 1");
        timeItem.IdTimeItemType = checkInType.IdTimeItemType;

        var service = new TimeItemService(context, new Legatro.DataLayer.Validators.TimeItemValidator(context), _fixture.Factory);

        await service.CreateAsync(timeItem);

        // Act - Multiple updates
        timeItem.ShortTitel = "Version 2";
        await service.UpdateAsync(timeItem);

        await System.Threading.Tasks.Task.Delay(100); // Small delay for different timestamps

        timeItem.ShortTitel = "Version 3";
        await service.UpdateAsync(timeItem);

        await System.Threading.Tasks.Task.Delay(100);

        timeItem.ShortTitel = "Version 4";
        await service.UpdateAsync(timeItem);

        // Assert
        var history = await service.GetHistoryAsync(timeItem.IdTimeItem);

        Assert.Equal(4, history.Count);
        Assert.Contains(history, h => h.ShortTitel == "Version 1");
        Assert.Contains(history, h => h.ShortTitel == "Version 2");
        Assert.Contains(history, h => h.ShortTitel == "Version 3");
        Assert.Contains(history, h => h.ShortTitel == "Version 4");
    }

    [Fact]
    public async System.Threading.Tasks.Task HistoricalVersion_HasNoLinkedListLinks()
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

        // Act - Update checkIn (has a next link)
        checkIn.ShortTitel = "Updated checkIn";
        await service.UpdateAsync(checkIn);

        // Assert
        var history = await service.GetHistoryAsync(checkIn.IdTimeItem);
        var historicalVersion = history.FirstOrDefault(h => h.IdTimeItem != checkIn.IdTimeItem && h.ShortTitel == "Test TimeItem");

        Assert.NotNull(historicalVersion);
        Assert.Null(historicalVersion.IdPreviousItem);
        Assert.Null(historicalVersion.IdNextItem);
        Assert.Null(historicalVersion.DurationToPrevious);
        Assert.Null(historicalVersion.DurationToNext);
    }
}

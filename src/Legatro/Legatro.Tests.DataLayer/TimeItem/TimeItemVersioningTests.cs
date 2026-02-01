using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Services.TimeItem;
using Legatro.Tests.DataLayer.Fixtures;

namespace Legatro.Tests.DataLayer.TimeItem;

/// <summary>
/// Tests for TimeItem versioning and history.
/// </summary>
public class TimeItemVersioningTests : IDisposable
{
    private readonly Legatro.DataLayer.Context.LegatroDbContext _context;
    private readonly TimeItemLinkedListManager _linkedListManager;
    private readonly TimeItemVersioningService _versioningService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _bookingDate = DateTime.UtcNow.Date;

    public TimeItemVersioningTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _linkedListManager = new TimeItemLinkedListManager(_context);
        _versioningService = new TimeItemVersioningService(_context, _linkedListManager);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Update_CreatesHistoryCopy_WithDateValidTo()
    {
        // Arrange
        var originalItem = CreateTimeItem(10, 0);
        _context.TimeItems.Add(originalItem);
        await _context.SaveChangesAsync();

        var originalId = originalItem.IdTimeItem;
        var beforeUpdate = DateTime.UtcNow;

        // Act
        var (archived, updated) = await _versioningService.CreateVersionAsync(
            originalItem,
            item => item.ShortTitel = "Updated Title");
        await _context.SaveChangesAsync();

        // Assert
        archived.DateValidTo.Should().NotBeNull();
        archived.DateValidTo.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public async Task Update_OriginalIdRemains_CopyGetsNewId()
    {
        // Arrange
        var originalItem = CreateTimeItem(10, 0);
        _context.TimeItems.Add(originalItem);
        await _context.SaveChangesAsync();

        var originalId = originalItem.IdTimeItem;

        // Act
        var (archived, updated) = await _versioningService.CreateVersionAsync(
            originalItem,
            item => item.ShortTitel = "Updated Title");
        await _context.SaveChangesAsync();

        // Assert
        updated.IdTimeItem.Should().Be(originalId);
        archived.IdTimeItem.Should().NotBe(originalId);
    }

    [Fact]
    public async Task Update_HistoryCopy_LinkedViaIdHistoryParent()
    {
        // Arrange
        var originalItem = CreateTimeItem(10, 0);
        _context.TimeItems.Add(originalItem);
        await _context.SaveChangesAsync();

        var originalId = originalItem.IdTimeItem;

        // Act
        var (archived, updated) = await _versioningService.CreateVersionAsync(
            originalItem,
            item => item.ShortTitel = "Updated Title");
        await _context.SaveChangesAsync();

        // Assert
        archived.IdHistoryParent.Should().Be(originalId);
    }

    [Fact]
    public async Task Update_AppliesChangesToOriginal()
    {
        // Arrange
        var originalItem = CreateTimeItem(10, 0);
        originalItem.ShortTitel = "Original Title";
        _context.TimeItems.Add(originalItem);
        await _context.SaveChangesAsync();

        // Act
        var (archived, updated) = await _versioningService.CreateVersionAsync(
            originalItem,
            item => item.ShortTitel = "Updated Title");
        await _context.SaveChangesAsync();

        // Assert
        updated.ShortTitel.Should().Be("Updated Title");
        archived.ShortTitel.Should().Be("Original Title");
    }

    [Fact]
    public async Task GetHistory_ReturnsAllVersions_InChronologicalOrder()
    {
        // Arrange
        var originalItem = CreateTimeItem(10, 0);
        originalItem.ShortTitel = "Version 1";
        _context.TimeItems.Add(originalItem);
        await _context.SaveChangesAsync();

        var originalId = originalItem.IdTimeItem;

        // Create version 2
        await _versioningService.CreateVersionAsync(
            originalItem,
            item => item.ShortTitel = "Version 2");
        await _context.SaveChangesAsync();

        await Task.Delay(10); // Small delay to ensure different timestamps

        // Create version 3
        await _versioningService.CreateVersionAsync(
            originalItem,
            item => item.ShortTitel = "Version 3");
        await _context.SaveChangesAsync();

        // Act
        var history = await _versioningService.GetHistoryAsync(originalId);

        // Assert
        history.Should().HaveCount(2); // 2 archived versions
        history[0].ShortTitel.Should().Be("Version 1");
        history[1].ShortTitel.Should().Be("Version 2");
    }

    [Fact]
    public async Task Update_ArchivedVersionNotInLinkedList()
    {
        // Arrange
        var originalItem = CreateTimeItem(10, 0);
        _context.TimeItems.Add(originalItem);
        await _linkedListManager.InsertIntoChainAsync(originalItem);
        await _context.SaveChangesAsync();

        // Act
        var (archived, updated) = await _versioningService.CreateVersionAsync(
            originalItem,
            item => item.ShortTitel = "Updated Title");
        await _context.SaveChangesAsync();

        // Assert - Archived version should not have linked list pointers
        archived.IdPreviousItem.Should().BeNull();
        archived.IdNextItem.Should().BeNull();
        archived.DurationToPrevious.Should().BeNull();
        archived.DurationToNext.Should().BeNull();
    }

    #region Helper Methods

    private Legatro.DataLayer.Entities.TimeItem CreateTimeItem(int hour, int minute)
    {
        var eventTime = new DateTimeOffset(_bookingDate.AddHours(hour).AddMinutes(minute), TimeSpan.Zero);

        return new Legatro.DataLayer.Entities.TimeItem
        {
            IdTimeItem = Guid.NewGuid(),
            IdUser = _userId,
            IdTimeItemType = TimeItemTypeIds.SetBooking,
            EventType = EventType.Time,
            EventTime = eventTime,
            BookingDateGMT = _bookingDate,
            ShortTitel = "Test Item"
        };
    }

    #endregion
}

using FluentAssertions;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Services.TimeItem;
using Legatro.Tests.DataLayer.Fixtures;

namespace Legatro.Tests.DataLayer.TimeItem;

/// <summary>
/// Tests for TimeItem linked list maintenance.
/// </summary>
public class TimeItemLinkedListTests : IDisposable
{
    private readonly Legatro.DataLayer.Context.LegatroDbContext _context;
    private readonly TimeItemLinkedListManager _manager;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _bookingDate = DateTime.UtcNow.Date;

    public TimeItemLinkedListTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _manager = new TimeItemLinkedListManager(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Insert_FirstItem_HasNoPreviousOrNext()
    {
        // Arrange
        var item = CreateTimeItem(8, 0);

        // Act
        _context.TimeItems.Add(item);
        await _manager.InsertIntoChainAsync(item);
        await _context.SaveChangesAsync();

        // Assert
        item.IdPreviousItem.Should().BeNull();
        item.IdNextItem.Should().BeNull();
        item.DurationToPrevious.Should().BeNull();
        item.DurationToNext.Should().BeNull();
    }

    [Fact]
    public async Task Insert_SecondItem_LinksToFirst()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item2 = CreateTimeItem(10, 0);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        await _context.SaveChangesAsync();

        // Act
        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        // Assert
        item1.IdNextItem.Should().Be(item2.IdTimeItem);
        item2.IdPreviousItem.Should().Be(item1.IdTimeItem);
    }

    [Fact]
    public async Task Insert_BetweenTwoItems_UpdatesBothLinks()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item3 = CreateTimeItem(12, 0);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        _context.TimeItems.Add(item3);
        await _manager.InsertIntoChainAsync(item3);
        await _context.SaveChangesAsync();

        // Act - Insert item2 between item1 and item3
        var item2 = CreateTimeItem(10, 0);
        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        // Assert
        item1.IdNextItem.Should().Be(item2.IdTimeItem);
        item2.IdPreviousItem.Should().Be(item1.IdTimeItem);
        item2.IdNextItem.Should().Be(item3.IdTimeItem);
        item3.IdPreviousItem.Should().Be(item2.IdTimeItem);
    }

    [Fact]
    public async Task Insert_UpdatesDurationToNext_OnPreviousItem()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item2 = CreateTimeItem(10, 0);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        await _context.SaveChangesAsync();

        // Act
        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        // Assert
        item1.DurationToNext.Should().Be(TimeSpan.FromHours(2));
        item1.DurationTicksToNext.Should().Be(TimeSpan.FromHours(2).Ticks);
    }

    [Fact]
    public async Task Insert_UpdatesDurationToPrevious_OnNextItem()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item2 = CreateTimeItem(10, 30);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        await _context.SaveChangesAsync();

        // Act
        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        // Assert
        item2.DurationToPrevious.Should().Be(TimeSpan.FromMinutes(150)); // 2.5 hours
        item2.DurationTicksToPrevious.Should().Be(TimeSpan.FromMinutes(150).Ticks);
    }

    [Fact]
    public async Task Delete_MiddleItem_LinksPreviousToNext()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item2 = CreateTimeItem(10, 0);
        var item3 = CreateTimeItem(12, 0);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        await _context.SaveChangesAsync();

        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        _context.TimeItems.Add(item3);
        await _manager.InsertIntoChainAsync(item3);
        await _context.SaveChangesAsync();

        // Act - Remove middle item
        await _manager.RemoveFromChainAsync(item2);
        await _context.SaveChangesAsync();

        // Assert
        item1.IdNextItem.Should().Be(item3.IdTimeItem);
        item3.IdPreviousItem.Should().Be(item1.IdTimeItem);
    }

    [Fact]
    public async Task Delete_RecalculatesDeltas_BetweenAdjacentItems()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item2 = CreateTimeItem(10, 0);
        var item3 = CreateTimeItem(12, 0);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        await _context.SaveChangesAsync();

        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        _context.TimeItems.Add(item3);
        await _manager.InsertIntoChainAsync(item3);
        await _context.SaveChangesAsync();

        // Act - Remove middle item
        await _manager.RemoveFromChainAsync(item2);
        await _context.SaveChangesAsync();

        // Assert - Duration from item1 to item3 should now be 4 hours
        item1.DurationToNext.Should().Be(TimeSpan.FromHours(4));
        item3.DurationToPrevious.Should().Be(TimeSpan.FromHours(4));
    }

    [Fact]
    public async Task Delete_FirstItem_UpdatesNextItemPreviousToNull()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item2 = CreateTimeItem(10, 0);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        await _context.SaveChangesAsync();

        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        // Verify chain is set up correctly before delete
        item1.IdNextItem.Should().Be(item2.IdTimeItem);
        item2.IdPreviousItem.Should().Be(item1.IdTimeItem);

        // Act - Remove first item
        await _manager.RemoveFromChainAsync(item1);
        await _context.SaveChangesAsync();

        // Assert
        item2.IdPreviousItem.Should().BeNull();
        item2.DurationToPrevious.Should().BeNull();
    }

    [Fact]
    public async Task Delete_LastItem_UpdatesPreviousItemNextToNull()
    {
        // Arrange
        var item1 = CreateTimeItem(8, 0);
        var item2 = CreateTimeItem(10, 0);

        _context.TimeItems.Add(item1);
        await _manager.InsertIntoChainAsync(item1);
        await _context.SaveChangesAsync();

        _context.TimeItems.Add(item2);
        await _manager.InsertIntoChainAsync(item2);
        await _context.SaveChangesAsync();

        // Verify chain is set up correctly before delete
        item1.IdNextItem.Should().Be(item2.IdTimeItem);
        item2.IdPreviousItem.Should().Be(item1.IdTimeItem);

        // Act - Remove last item
        await _manager.RemoveFromChainAsync(item2);
        await _context.SaveChangesAsync();

        // Assert
        item1.IdNextItem.Should().BeNull();
        item1.DurationToNext.Should().BeNull();
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
            ShortTitel = $"Item at {hour:D2}:{minute:D2}"
        };
    }

    #endregion
}

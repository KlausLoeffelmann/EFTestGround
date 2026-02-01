using FluentAssertions;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;
using Legatro.DataLayer.Validators;
using Legatro.Tests.DataLayer.Fixtures;

namespace Legatro.Tests.DataLayer.TimeItem;

/// <summary>
/// Tests for TimeItem plausibility validation rules.
/// </summary>
public class TimeItemValidatorTests
{
    private readonly TimeItemValidator _validator;

    public TimeItemValidatorTests()
    {
        using var context = InMemoryDbContextFactory.Create();
        _validator = new TimeItemValidator(context);
    }

    #region First Event of Day Tests

    [Fact]
    public async Task FirstEventMustBeCheckIn_WhenNoEventsExist_CheckInSucceeds()
    {
        // Arrange
        var checkInItem = CreateTimeItem(TimeItemBookingType.CheckIn);

        // Act
        var result = await _validator.ValidateAsync(checkInItem, lastEvent: null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.RequiresAutoInsertCheckIn.Should().BeFalse();
    }

    [Fact]
    public async Task FirstEventMustBeCheckIn_WhenNoEventsExist_ThrowsOnCheckOut()
    {
        // Arrange
        var checkOutItem = CreateTimeItem(TimeItemBookingType.CheckOut);

        // Act
        var result = await _validator.ValidateAsync(checkOutItem, lastEvent: null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("CheckOut");
        result.ErrorMessage.Should().Contain("first event");
    }

    [Fact]
    public async Task FirstEventMustBeCheckIn_WhenNoEventsExist_AutoInsertsCheckInBeforeBreak()
    {
        // Arrange
        var breakItem = CreateTimeItem(TimeItemBookingType.Break);

        // Act
        var result = await _validator.ValidateAsync(breakItem, lastEvent: null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.RequiresAutoInsertCheckIn.Should().BeTrue();
    }

    [Fact]
    public async Task FirstEventMustBeCheckIn_WhenNoEventsExist_AutoInsertsCheckInBeforeDowntime()
    {
        // Arrange
        var downtimeItem = CreateTimeItem(TimeItemBookingType.Downtime);

        // Act
        var result = await _validator.ValidateAsync(downtimeItem, lastEvent: null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.RequiresAutoInsertCheckIn.Should().BeTrue();
    }

    [Fact]
    public async Task FirstEventMustBeCheckIn_WhenNoEventsExist_AutoInsertsCheckInBeforeBusinessErrand()
    {
        // Arrange
        var errandItem = CreateTimeItem(TimeItemBookingType.BusinessErrand);

        // Act
        var result = await _validator.ValidateAsync(errandItem, lastEvent: null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.RequiresAutoInsertCheckIn.Should().BeTrue();
    }

    [Fact]
    public async Task FirstEventMustBeCheckIn_WhenNoEventsExist_AutoInsertsCheckInBeforeSetBooking()
    {
        // Arrange
        var bookingItem = CreateTimeItem(TimeItemBookingType.SetBooking);

        // Act
        var result = await _validator.ValidateAsync(bookingItem, lastEvent: null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.RequiresAutoInsertCheckIn.Should().BeTrue();
    }

    #endregion

    #region After CheckIn Transition Tests

    [Fact]
    public async Task CheckOut_AfterCheckIn_Succeeds()
    {
        // Arrange
        var lastCheckIn = CreateTimeItem(TimeItemBookingType.CheckIn);
        var newCheckOut = CreateTimeItem(TimeItemBookingType.CheckOut);

        // Act
        var result = await _validator.ValidateAsync(newCheckOut, lastCheckIn);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Break_AfterCheckIn_Succeeds()
    {
        // Arrange
        var lastCheckIn = CreateTimeItem(TimeItemBookingType.CheckIn);
        var newBreak = CreateTimeItem(TimeItemBookingType.Break);

        // Act
        var result = await _validator.ValidateAsync(newBreak, lastCheckIn);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task SetBooking_AfterCheckIn_Succeeds()
    {
        // Arrange
        var lastCheckIn = CreateTimeItem(TimeItemBookingType.CheckIn);
        var newBooking = CreateTimeItem(TimeItemBookingType.SetBooking);

        // Act
        var result = await _validator.ValidateAsync(newBooking, lastCheckIn);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region After CheckOut Transition Tests

    [Fact]
    public async Task CheckOut_AfterCheckOut_ThrowsException()
    {
        // Arrange
        var lastCheckOut = CreateTimeItem(TimeItemBookingType.CheckOut);
        var newCheckOut = CreateTimeItem(TimeItemBookingType.CheckOut);

        // Act
        var result = await _validator.ValidateAsync(newCheckOut, lastCheckOut);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot transition");
    }

    [Fact]
    public async Task CheckIn_AfterCheckOut_Succeeds()
    {
        // Arrange
        var lastCheckOut = CreateTimeItem(TimeItemBookingType.CheckOut);
        var newCheckIn = CreateTimeItem(TimeItemBookingType.CheckIn);

        // Act
        var result = await _validator.ValidateAsync(newCheckIn, lastCheckOut);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Break_AfterCheckOut_ThrowsException()
    {
        // Arrange
        var lastCheckOut = CreateTimeItem(TimeItemBookingType.CheckOut);
        var newBreak = CreateTimeItem(TimeItemBookingType.Break);

        // Act
        var result = await _validator.ValidateAsync(newBreak, lastCheckOut);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task SetBooking_AfterCheckOut_ThrowsException()
    {
        // Arrange
        var lastCheckOut = CreateTimeItem(TimeItemBookingType.CheckOut);
        var newBooking = CreateTimeItem(TimeItemBookingType.SetBooking);

        // Act
        var result = await _validator.ValidateAsync(newBooking, lastCheckOut);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region After Break Transition Tests

    [Fact]
    public async Task CheckIn_AfterBreak_Succeeds()
    {
        // Arrange
        var lastBreak = CreateTimeItem(TimeItemBookingType.Break);
        var newCheckIn = CreateTimeItem(TimeItemBookingType.CheckIn);

        // Act
        var result = await _validator.ValidateAsync(newCheckIn, lastBreak);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CheckOut_AfterBreak_Succeeds()
    {
        // Arrange
        var lastBreak = CreateTimeItem(TimeItemBookingType.Break);
        var newCheckOut = CreateTimeItem(TimeItemBookingType.CheckOut);

        // Act
        var result = await _validator.ValidateAsync(newCheckOut, lastBreak);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static Legatro.DataLayer.Entities.TimeItem CreateTimeItem(TimeItemBookingType bookingType)
    {
        var timeItemTypeId = bookingType switch
        {
            TimeItemBookingType.Default => TimeItemTypeIds.Default,
            TimeItemBookingType.CheckIn => TimeItemTypeIds.CheckIn,
            TimeItemBookingType.CheckOut => TimeItemTypeIds.CheckOut,
            TimeItemBookingType.Break => TimeItemTypeIds.Break,
            TimeItemBookingType.Downtime => TimeItemTypeIds.Downtime,
            TimeItemBookingType.BusinessErrand => TimeItemTypeIds.BusinessErrand,
            TimeItemBookingType.SetBooking => TimeItemTypeIds.SetBooking,
            _ => TimeItemTypeIds.Default
        };

        return new Legatro.DataLayer.Entities.TimeItem
        {
            IdTimeItem = Guid.NewGuid(),
            IdUser = Guid.NewGuid(),
            IdTimeItemType = timeItemTypeId,
            EventType = EventType.Time,
            EventTime = DateTimeOffset.UtcNow,
            BookingDateGMT = DateTime.UtcNow.Date
        };
    }

    #endregion
}

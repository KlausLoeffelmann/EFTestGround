using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Legatro.DataLayer.Entities;
using Legatro.DataLayer.Enums;

namespace Legatro.DataLayer.Configuration;

public class TimeItemTypeConfiguration : IEntityTypeConfiguration<TimeItemType>
{
    public void Configure(EntityTypeBuilder<TimeItemType> builder)
    {
        builder.HasKey(e => e.IdTimeItemType);

        builder.HasIndex(e => e.SyncGuid).IsUnique();

        builder.Property(e => e.TimeItemTypeName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ShortName).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(255).IsRequired();

        // Store BookingType as its underlying short type
        builder.Property(e => e.BookingType)
            .HasConversion<short>();

        // Seed system types
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            CreateSystemType(TimeItemTypeIds.Default, "Default", "Default", 0,
                TimeItemBookingType.Default,
                "Used in the flat time table for all entries related to quantity capture timestamps.",
                now),
            CreateSystemType(TimeItemTypeIds.CheckIn, "Check In", "CheckIn", 1,
                TimeItemBookingType.CheckIn,
                "Called when an employee clocks in.",
                now),
            CreateSystemType(TimeItemTypeIds.CheckOut, "Check Out", "CheckOut", 2,
                TimeItemBookingType.CheckOut,
                "Called when an employee clocks out.",
                now),
            CreateSystemType(TimeItemTypeIds.Break, "Break", "Break", 3,
                TimeItemBookingType.Break,
                "Employee books an unspecified break.",
                now),
            CreateSystemType(TimeItemTypeIds.Downtime, "Downtime", "Downtime", 4,
                TimeItemBookingType.Downtime,
                "Downtime booked when employee cannot continue working for operational reasons.",
                now),
            CreateSystemType(TimeItemTypeIds.BusinessErrand, "Business Errand", "BizErrand", 5,
                TimeItemBookingType.BusinessErrand,
                "Unspecified business errand outside the office.",
                now),
            CreateSystemType(TimeItemTypeIds.SetBooking, "Set Booking", "SetBooking", 6,
                TimeItemBookingType.SetBooking,
                "Booking to a target set that can contain multiple projects, orders, products, or combinations.",
                now)
        );
    }

    private static TimeItemType CreateSystemType(
        Guid id,
        string name,
        string shortName,
        int displayOrder,
        TimeItemBookingType bookingType,
        string description,
        DateTime date)
    {
        return new TimeItemType
        {
            IdTimeItemType = id,
            TimeItemTypeName = name,
            ShortName = shortName,
            DisplayOrder = displayOrder,
            IsSystemType = true,
            BookingType = bookingType,
            Description = description,
            SyncGuid = id, // Use same GUID for SyncGuid in seed data
            DateCreated = date,
            DateLastEdited = date
        };
    }
}

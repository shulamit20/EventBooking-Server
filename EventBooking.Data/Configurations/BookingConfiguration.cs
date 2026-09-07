using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>Fluent API mapping for <see cref="Booking"/>.</summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.EventType).IsRequired().HasMaxLength(100);
        builder.Property(b => b.HostName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.Notes).HasMaxLength(2000);

        // FKs (HallSlot, Owner) are configured on the HallSlot / User side.

        // Helps the "is this slot already taken?" query.
        builder.HasIndex(b => new { b.HallSlotId, b.Status });

        // Helps the "my bookings" screen: WHERE OwnerUserId = @id ORDER BY CreatedAtUtc DESC.
        // Filter column ascending, sort column descending — matches the query exactly.
        builder.HasIndex(b => new { b.OwnerUserId, b.CreatedAtUtc })
            .IsDescending(false, true);
    }
}

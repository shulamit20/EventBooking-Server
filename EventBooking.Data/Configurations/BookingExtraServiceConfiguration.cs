using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>
/// Fluent API mapping for the <see cref="BookingExtraService"/> join entity —
/// the many-to-many between <see cref="Booking"/> and <see cref="ExtraService"/> with payload.
/// </summary>
public class BookingExtraServiceConfiguration : IEntityTypeConfiguration<BookingExtraService>
{
    public void Configure(EntityTypeBuilder<BookingExtraService> builder)
    {
        // Composite primary key.
        builder.HasKey(bes => new { bes.BookingId, bes.ExtraServiceId });

        builder.Property(bes => bes.PriceAtBooking).HasPrecision(10, 2);
        builder.Property(bes => bes.LineTotal).HasPrecision(12, 2);

        builder.HasOne(bes => bes.Booking)
               .WithMany(b => b.BookingExtraServices)
               .HasForeignKey(bes => bes.BookingId)
               .OnDelete(DeleteBehavior.Cascade); // deleting a booking removes its line items

        builder.HasOne(bes => bes.ExtraService)
               .WithMany(e => e.BookingExtraServices)
               .HasForeignKey(bes => bes.ExtraServiceId)
               .OnDelete(DeleteBehavior.Restrict); // can't delete a service that bookings reference
    }
}

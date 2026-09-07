using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="HallSlot"/> — the limited resource.
/// </summary>
public class HallSlotConfiguration : IEntityTypeConfiguration<HallSlot>
{
    public void Configure(EntityTypeBuilder<HallSlot> builder)
    {
        builder.HasKey(s => s.Id);

        // Calendar date only (no time-of-day). Avoids the Npgsql "timestamp must be UTC" trap for this column.
        builder.Property(s => s.Date).HasColumnType("date");

        builder.Property(s => s.Shift).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(s => s.BasePrice).HasPrecision(10, 2);

        // Optimistic concurrency token (self-managed Guid, PostgreSQL "second way").
        // EF adds the ORIGINAL value to the UPDATE ... WHERE clause. A stale write updates
        // zero rows and throws DbUpdateConcurrencyException. The new value is assigned in
        // AppDbContext.SaveChangesAsync.
        builder.Property(s => s.Version).IsConcurrencyToken();

        // One hall cannot have two slots for the same date + shift.
        builder.HasIndex(s => new { s.HallId, s.Date, s.Shift }).IsUnique();

        // One-to-many: HallSlot 1 -> * Booking.
        builder.HasMany(s => s.Bookings)
               .WithOne(b => b.HallSlot)
               .HasForeignKey(b => b.HallSlotId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

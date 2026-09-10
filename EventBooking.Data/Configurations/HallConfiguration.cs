using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>Fluent API mapping for <see cref="Hall"/>.</summary>
public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Description).HasMaxLength(1000);

        builder.Property(h => h.MorningPrice).HasPrecision(10, 2);
        builder.Property(h => h.NoonPrice).HasPrecision(10, 2);
        builder.Property(h => h.EveningPrice).HasPrecision(10, 2);

        // One-to-many: Hall 1 -> * HallSlot.
        builder.HasMany(h => h.Slots)
               .WithOne(s => s.Hall)
               .HasForeignKey(s => s.HallId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.VenueId, h.Name }).IsUnique(); // no two halls with the same name in one venue
    }
}

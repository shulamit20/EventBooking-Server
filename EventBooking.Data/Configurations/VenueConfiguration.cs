using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>Fluent API mapping for <see cref="Venue"/>.</summary>
public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.City).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Address).IsRequired().HasMaxLength(300);
        builder.Property(v => v.ContactPhone).HasMaxLength(30);
        builder.Property(v => v.Description).HasMaxLength(1000);

        // One-to-many: Venue 1 -> * Hall. Configured fully on the Hall side.
        builder.HasMany(v => v.Halls)
               .WithOne(h => h.Venue)
               .HasForeignKey(h => h.VenueId)
               .OnDelete(DeleteBehavior.Restrict); // don't wipe halls (and their bookings) if a venue is deleted
    }
}

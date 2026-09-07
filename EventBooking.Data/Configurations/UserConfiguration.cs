using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>Fluent API mapping for <see cref="User"/>.</summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        // One-to-many: User 1 -> * Booking (the client who made it).
        builder.HasMany(u => u.Bookings)
               .WithOne(b => b.Owner)
               .HasForeignKey(b => b.OwnerUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

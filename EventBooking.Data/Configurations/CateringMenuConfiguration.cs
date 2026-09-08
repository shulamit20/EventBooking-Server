using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>Fluent API mapping for <see cref="CateringMenu"/>.</summary>
public class CateringMenuConfiguration : IEntityTypeConfiguration<CateringMenu>
{
    public void Configure(EntityTypeBuilder<CateringMenu> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(1000);
        builder.Property(m => m.PricePerGuest).HasPrecision(10, 2);

        builder.HasOne(m => m.Owner)
               .WithMany(u => u.OwnedCateringMenus)
               .HasForeignKey(m => m.OwnerUserId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(m => m.OwnerUserId);
    }
}

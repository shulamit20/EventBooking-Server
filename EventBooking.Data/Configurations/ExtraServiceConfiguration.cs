using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>Fluent API mapping for <see cref="ExtraService"/>.</summary>
public class ExtraServiceConfiguration : IEntityTypeConfiguration<ExtraService>
{
    public void Configure(EntityTypeBuilder<ExtraService> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.UnitLabel).HasMaxLength(50);
        builder.Property(e => e.ImageUrl).HasMaxLength(500);
        builder.Property(e => e.Price).HasPrecision(10, 2);
        builder.Property(e => e.Pricing).HasConversion<string>().HasMaxLength(20);

        // ExtraService * -> 1 ServiceCategory
        builder.HasOne(e => e.ServiceCategory)
               .WithMany(c => c.Services)
               .HasForeignKey(e => e.ServiceCategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // ExtraService * -> 1 User (the Manager who owns it)
        builder.HasOne(e => e.Owner)
               .WithMany(u => u.OwnedServices)
               .HasForeignKey(e => e.OwnerUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ServiceCategoryId);
        builder.HasIndex(e => e.OwnerUserId);
    }
}

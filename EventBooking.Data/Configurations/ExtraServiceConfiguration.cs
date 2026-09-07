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
        builder.Property(e => e.Price).HasPrecision(10, 2);
    }
}

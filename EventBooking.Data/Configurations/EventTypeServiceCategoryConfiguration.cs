using EventBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="EventTypeServiceCategory"/> — the many-to-many linking an
/// <see cref="EventType"/> to the <see cref="ServiceCategory"/> options offered for it.
/// </summary>
public class EventTypeServiceCategoryConfiguration : IEntityTypeConfiguration<EventTypeServiceCategory>
{
    public void Configure(EntityTypeBuilder<EventTypeServiceCategory> builder)
    {
        builder.HasKey(x => new { x.EventTypeId, x.ServiceCategoryId });

        builder.HasOne(x => x.EventType)
               .WithMany(t => t.ServiceCategories)
               .HasForeignKey(x => x.EventTypeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServiceCategory)
               .WithMany(c => c.EventTypes)
               .HasForeignKey(x => x.ServiceCategoryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

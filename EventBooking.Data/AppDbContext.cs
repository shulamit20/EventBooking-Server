using EventBooking.Core.Entities;
using EventBooking.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data;

/// <summary>
/// The single EF Core unit of work for the whole system.
/// One instance per HTTP request (registered as Scoped in Program.cs).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // One DbSet per entity. Each becomes a table.
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<HallSlot> HallSlots => Set<HallSlot>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<ExtraService> ExtraServices => Set<ExtraService>();
    public DbSet<BookingExtraService> BookingExtraServices => Set<BookingExtraService>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<EventTypeServiceCategory> EventTypeServiceCategories => Set<EventTypeServiceCategory>();
    public DbSet<CateringMenu> CateringMenus => Set<CateringMenu>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration<T> in this assembly (Configurations/ folder).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Catalog seed data (venues, halls, slots, extra services) baked into the migration.
        SeedData.Apply(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        BumpConcurrencyTokens();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        BumpConcurrencyTokens();
        return base.SaveChanges();
    }

    /// <summary>
    /// Gives every modified <see cref="HallSlot"/> a fresh <see cref="HallSlot.Version"/> before save.
    /// Done here once, so no service has to remember it. EF still compares the ORIGINAL value in the
    /// WHERE clause, so a write based on stale data updates zero rows -> DbUpdateConcurrencyException.
    /// </summary>
    private void BumpConcurrencyTokens()
    {
        foreach (var entry in ChangeTracker.Entries<HallSlot>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.Version = Guid.NewGuid();
        }
    }
}

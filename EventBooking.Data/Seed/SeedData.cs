using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Seed;

/// <summary>
/// Catalog + demo-account seed data, baked into migrations via <c>HasData</c>, so a fresh
/// database is usable immediately after <c>dotnet ef database update</c> — no manual inserts.
///
/// Rules for <c>HasData</c>: every value must be a compile-time constant. No <c>DateTime.Now</c>,
/// no <c>Guid.NewGuid()</c>. Password hashes are pre-computed (a valid BCrypt hash of
/// "Passw0rd!") so the seed does not need the runtime hasher.
/// </summary>
internal static class SeedData
{
    // --- fixed identifiers so relationships can be seeded ---
    public static readonly Guid AdminId    = new("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid ManagerId  = new("b0000000-0000-0000-0000-000000000002");
    public static readonly Guid CustomerId = new("c0000000-0000-0000-0000-000000000003");

    // BCrypt hash of "Passw0rd!" (work factor 12). Same password for all three demo accounts.
    private const string DemoHash = "$2a$12$./ujlozjB7mpUpkNuPnKxOQEuUb/FVNPvZ1qYkSDWAD5Af8Sn0JtC";
    private static readonly DateTime Seeded = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Fixed concurrency-token values for the seeded slots.
    private static readonly Guid V1 = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid V2 = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid V3 = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid V4 = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid V5 = new("55555555-5555-5555-5555-555555555555");
    private static readonly Guid V6 = new("66666666-6666-6666-6666-666666666666");
    private static readonly Guid V7 = new("77777777-7777-7777-7777-777777777777");

    public static void Apply(ModelBuilder modelBuilder)
    {
        // --- demo accounts (one per role) ---
        modelBuilder.Entity<User>().HasData(
            new User { Id = AdminId,    Email = "admin@eventbooking.local",   DisplayName = "System Admin",  Role = UserRole.Admin,    PasswordHash = DemoHash, CreatedAtUtc = Seeded },
            new User { Id = ManagerId,  Email = "manager@eventbooking.local", DisplayName = "Demo Manager",  Role = UserRole.Manager,  PasswordHash = DemoHash, CreatedAtUtc = Seeded },
            new User { Id = CustomerId, Email = "client@eventbooking.local",  DisplayName = "Demo Customer", Role = UserRole.Customer, PasswordHash = DemoHash, CreatedAtUtc = Seeded });

        // --- lookup: event types ---
        modelBuilder.Entity<EventType>().HasData(
            new EventType { Id = 1, Name = "Wedding" },
            new EventType { Id = 2, Name = "Bar Mitzvah" },
            new EventType { Id = 3, Name = "Bat Mitzvah" },
            new EventType { Id = 4, Name = "Corporate Event" },
            new EventType { Id = 5, Name = "Birthday" },
            new EventType { Id = 6, Name = "Private Event" },
            new EventType { Id = 7, Name = "Other" });

        // --- lookup: service categories ---
        modelBuilder.Entity<ServiceCategory>().HasData(
            new ServiceCategory { Id = 1, Code = "Catering",     Name = "Catering" },
            new ServiceCategory { Id = 2, Code = "TableDesign",  Name = "Table Design" },
            new ServiceCategory { Id = 3, Code = "BridalChair",  Name = "Bridal Chair" },
            new ServiceCategory { Id = 4, Code = "Photography",  Name = "Photography" },
            new ServiceCategory { Id = 5, Code = "DJ",           Name = "DJ / Music" },
            new ServiceCategory { Id = 6, Code = "Flowers",      Name = "Flowers" },
            new ServiceCategory { Id = 7, Code = "Lighting",     Name = "Lighting" },
            new ServiceCategory { Id = 8, Code = "Other",        Name = "Other" });

        // --- which categories are offered for which event type ---
        modelBuilder.Entity<EventTypeServiceCategory>().HasData(BuildEventTypeLinks());

        // --- venues (owned by the demo manager) ---
        modelBuilder.Entity<Venue>().HasData(
            new Venue { Id = 1, OwnerUserId = ManagerId, Name = "Beit Simcha", City = "Jerusalem", Address = "Rehov Malchei Yisrael 12", ContactPhone = "02-500-1000" },
            new Venue { Id = 2, OwnerUserId = ManagerId, Name = "Ganei HaPnina", City = "Bnei Brak", Address = "Rehov Rabbi Akiva 88", ContactPhone = "03-570-2000" });

        modelBuilder.Entity<Hall>().HasData(
            new Hall { Id = 1, VenueId = 1, Name = "Main Ballroom", Capacity = 400 },
            new Hall { Id = 2, VenueId = 1, Name = "Garden Hall", Capacity = 150 },
            new Hall { Id = 3, VenueId = 2, Name = "Crystal Hall", Capacity = 300 });

        modelBuilder.Entity<HallSlot>().HasData(
            new HallSlot { Id = 1, HallId = 1, Date = new DateTime(2026, 10, 1), Shift = ShiftType.Morning, BasePrice = 6000m, Status = SlotStatus.Available, Version = V1 },
            new HallSlot { Id = 2, HallId = 1, Date = new DateTime(2026, 10, 1), Shift = ShiftType.Noon, BasePrice = 9000m, Status = SlotStatus.Available, Version = V2 },
            new HallSlot { Id = 3, HallId = 1, Date = new DateTime(2026, 10, 1), Shift = ShiftType.Evening, BasePrice = 15000m, Status = SlotStatus.Available, Version = V3 },
            new HallSlot { Id = 4, HallId = 1, Date = new DateTime(2026, 10, 2), Shift = ShiftType.Evening, BasePrice = 15000m, Status = SlotStatus.Available, Version = V4 },
            new HallSlot { Id = 5, HallId = 2, Date = new DateTime(2026, 10, 1), Shift = ShiftType.Evening, BasePrice = 8000m, Status = SlotStatus.Available, Version = V5 },
            new HallSlot { Id = 6, HallId = 3, Date = new DateTime(2026, 10, 5), Shift = ShiftType.Noon, BasePrice = 10000m, Status = SlotStatus.Available, Version = V6 },
            new HallSlot { Id = 7, HallId = 3, Date = new DateTime(2026, 10, 5), Shift = ShiftType.Evening, BasePrice = 16000m, Status = SlotStatus.Available, Version = V7 });

        // --- extra services (owned by the demo manager) ---
        modelBuilder.Entity<ExtraService>().HasData(
            new ExtraService { Id = 1, OwnerUserId = ManagerId, ServiceCategoryId = 1, Pricing = PricingModel.PerGuest, Name = "Catering - Meat Menu", Price = 220m, UnitLabel = "per guest", Description = "Full meat menu, first course to dessert." },
            new ExtraService { Id = 2, OwnerUserId = ManagerId, ServiceCategoryId = 6, Pricing = PricingModel.Flat,     Name = "Floral Centerpieces", Price = 180m, UnitLabel = "per table" },
            new ExtraService { Id = 3, OwnerUserId = ManagerId, ServiceCategoryId = 5, Pricing = PricingModel.Flat,     Name = "Live Band", Price = 8000m, UnitLabel = "per event" },
            new ExtraService { Id = 4, OwnerUserId = ManagerId, ServiceCategoryId = 4, Pricing = PricingModel.Flat,     Name = "Photography", Price = 5000m, UnitLabel = "per event" });
    }

    /// <summary>Sensible event-type → service-category offerings.</summary>
    private static EventTypeServiceCategory[] BuildEventTypeLinks()
    {
        // categoryId 3 = BridalChair — only weddings.
        var byType = new Dictionary<int, int[]>
        {
            [1] = new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, // Wedding — all
            [2] = new[] { 1, 2, 4, 5, 6, 7, 8 },    // Bar Mitzvah
            [3] = new[] { 1, 2, 4, 5, 6, 7, 8 },    // Bat Mitzvah
            [4] = new[] { 1, 4, 5, 7, 8 },          // Corporate Event
            [5] = new[] { 1, 2, 4, 5, 6, 8 },       // Birthday
            [6] = new[] { 1, 2, 4, 5, 6, 7, 8 },    // Private Event
            [7] = new[] { 1, 2, 4, 5, 6, 7, 8 },    // Other
        };

        return byType
            .SelectMany(kv => kv.Value.Select(cat => new EventTypeServiceCategory
            {
                EventTypeId = kv.Key,
                ServiceCategoryId = cat,
            }))
            .ToArray();
    }
}

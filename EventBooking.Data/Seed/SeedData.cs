using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Seed;

/// <summary>
/// Catalog seed data baked into the Init migration via <c>HasData</c>, so a fresh database
/// is usable immediately after <c>dotnet ef database update</c> — no manual inserts.
///
/// Rules for <c>HasData</c>: every value must be a compile-time constant. No <c>DateTime.Now</c>,
/// no <c>Guid.NewGuid()</c> — those would make the migration differ on every generation.
///
/// Demo <see cref="User"/> accounts are seeded separately at runtime (Step H), once the
/// password hasher exists.
/// </summary>
internal static class SeedData
{
    // Fixed concurrency-token values for the seeded slots (constants, not Guid.NewGuid()).
    private static readonly Guid V1 = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid V2 = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid V3 = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid V4 = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid V5 = new("55555555-5555-5555-5555-555555555555");
    private static readonly Guid V6 = new("66666666-6666-6666-6666-666666666666");
    private static readonly Guid V7 = new("77777777-7777-7777-7777-777777777777");

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>().HasData(
            new Venue { Id = 1, Name = "Beit Simcha", City = "Jerusalem", Address = "Rehov Malchei Yisrael 12", ContactPhone = "02-500-1000" },
            new Venue { Id = 2, Name = "Ganei HaPnina", City = "Bnei Brak", Address = "Rehov Rabbi Akiva 88", ContactPhone = "03-570-2000" });

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

        modelBuilder.Entity<ExtraService>().HasData(
            new ExtraService { Id = 1, Name = "Catering - Meat Menu", Price = 220m, UnitLabel = "per plate", Description = "Full meat menu, first course to dessert." },
            new ExtraService { Id = 2, Name = "Floral Centerpieces", Price = 180m, UnitLabel = "per table" },
            new ExtraService { Id = 3, Name = "Live Band", Price = 8000m, UnitLabel = "per event" },
            new ExtraService { Id = 4, Name = "Photography", Price = 5000m, UnitLabel = "per event" });
    }
}

using EventBooking.Core.Enums;
using EventBooking.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Tests.Data;

/// <summary>
/// Part C, evaluated separately: proves the <see cref="Core.Entities.HallSlot.Version"/>
/// concurrency token actually stops a lost race. Two real <see cref="AppDbContext"/> instances
/// read the same slot; the first write wins, the second throws
/// <see cref="DbUpdateConcurrencyException"/>.
///
/// Runs on an in-memory SQLite database (real SQL, real UPDATE ... WHERE Version = @original —
/// something the EF in-memory provider cannot do). xUnit creates a fresh instance, and therefore
/// a fresh database, per test.
/// </summary>
public class HallSlotConcurrencyTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public HallSlotConcurrencyTests()
    {
        // The in-memory database lives exactly as long as this open connection.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = new AppDbContext(_options);
        ctx.Database.EnsureCreated(); // schema from the model + the HasData catalog seed (slots 1..7)
    }

    [Fact]
    public void TwoUsersBookingTheSameSlot_TheSecondSaveThrowsConcurrencyException()
    {
        using var ctxA = new AppDbContext(_options);
        using var ctxB = new AppDbContext(_options);

        // Both users load slot 1 while it is still Available.
        var slotA = ctxA.HallSlots.Single(s => s.Id == 1);
        var slotB = ctxB.HallSlots.Single(s => s.Id == 1);

        // User A books first — succeeds, and SaveChanges bumps Version.
        slotA.Status = SlotStatus.Booked;
        ctxA.SaveChanges();

        // User B books the slot they still believe is Available — the WHERE clause carries the
        // now-stale original Version, matches zero rows, and EF throws.
        slotB.Status = SlotStatus.Booked;

        Assert.Throws<DbUpdateConcurrencyException>(() => ctxB.SaveChanges());
    }

    [Fact]
    public void AfterReloading_TheNextUpdateSucceeds()
    {
        using (var first = new AppDbContext(_options))
        {
            var slot = first.HallSlots.Single(s => s.Id == 2);
            slot.Status = SlotStatus.Booked;
            first.SaveChanges();
        }

        // A fresh read picks up the new Version, so a following update is not a conflict.
        using var second = new AppDbContext(_options);
        var reloaded = second.HallSlots.Single(s => s.Id == 2);
        reloaded.Status = SlotStatus.Available;

        var rowsAffected = second.SaveChanges();

        Assert.Equal(1, rowsAffected);
    }

    public void Dispose() => _connection.Dispose();
}

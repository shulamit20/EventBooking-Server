using EventBooking.Core.Enums;

namespace EventBooking.Core.Entities;

/// <summary>
/// The limited resource. One hall, one date, one shift.
/// Several clients may try to book the same slot at the same moment;
/// only one may succeed. Optimistic concurrency is enforced through <see cref="Version"/>.
/// </summary>
public class HallSlot
{
    public int Id { get; set; }

    // Foreign key: HallSlot * -> 1 Hall
    public int HallId { get; set; }
    public Hall Hall { get; set; } = null!;

    /// <summary>Calendar date of the slot, stored in UTC.</summary>
    public DateTime Date { get; set; }

    public ShiftType Shift { get; set; }

    public decimal BasePrice { get; set; }

    public SlotStatus Status { get; set; } = SlotStatus.Available;

    /// <summary>
    /// Concurrency token. Changed on every update (see DbContext.SaveChangesAsync override).
    /// EF Core adds its previous value to the UPDATE ... WHERE clause, so a write based on
    /// stale data updates zero rows and throws DbUpdateConcurrencyException.
    /// </summary>
    public Guid Version { get; set; } = Guid.NewGuid();

    // Navigation: HallSlot 1 -> * Booking (business rule: at most one non-cancelled booking).
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

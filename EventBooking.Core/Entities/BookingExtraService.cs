namespace EventBooking.Core.Entities;

/// <summary>
/// Join entity for the many-to-many between <see cref="Booking"/> and <see cref="ExtraService"/>.
/// Carries payload: how many units were ordered, the unit price captured at booking time, and
/// the resulting line total (both written by the server-side price calculator).
/// Composite primary key (BookingId, ExtraServiceId) is configured with Fluent API.
/// </summary>
/// <remarks>
/// Named <c>BookingExtraService</c> (not <c>BookingService</c>) so it does not collide with the
/// booking service class in the Service layer.
/// </remarks>
public class BookingExtraService
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int ExtraServiceId { get; set; }
    public ExtraService ExtraService { get; set; } = null!;

    public int Quantity { get; set; } = 1;

    /// <summary>Snapshot of <see cref="ExtraService.Price"/> when this line was priced.</summary>
    public decimal PriceAtBooking { get; set; }

    /// <summary>
    /// Snapshot of the line total = unit price × quantity (Flat) or × guest count (PerGuest).
    /// Stored, not recomputed, so a later price change never rewrites history.
    /// </summary>
    public decimal LineTotal { get; set; }
}

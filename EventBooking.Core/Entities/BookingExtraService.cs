namespace EventBooking.Core.Entities;

/// <summary>
/// Join entity for the many-to-many between <see cref="Booking"/> and <see cref="ExtraService"/>.
/// Carries payload: how many units were ordered and the price captured at booking time.
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

    /// <summary>Snapshot of <see cref="ExtraService.Price"/> when this line was added.</summary>
    public decimal PriceAtBooking { get; set; }
}

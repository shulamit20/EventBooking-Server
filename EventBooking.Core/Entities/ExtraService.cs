namespace EventBooking.Core.Entities;

/// <summary>An add-on a client can attach to a booking (catering, decor, sound, etc.).</summary>
public class ExtraService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }

    /// <summary>How the price is counted, e.g. "per plate", "per event".</summary>
    public string? UnitLabel { get; set; }

    // Navigation: many-to-many with Booking, through the BookingExtraService join entity.
    public ICollection<BookingExtraService> BookingExtraServices { get; set; } = new List<BookingExtraService>();
}

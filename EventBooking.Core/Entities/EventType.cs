namespace EventBooking.Core.Entities;

/// <summary>
/// A kind of event a customer can plan (Wedding, Bar Mitzvah, ...). A lookup table, not an
/// enum: no code branches on the value — it only drives which service categories are offered
/// (via <see cref="EventTypeServiceCategory"/>) — so an Admin can add a new type without a
/// redeploy.
/// </summary>
public class EventType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    /// <summary>Hidden from customers when false (kept for historical bookings).</summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<EventTypeServiceCategory> ServiceCategories { get; set; } = new List<EventTypeServiceCategory>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

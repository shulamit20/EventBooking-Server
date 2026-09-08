namespace EventBooking.Core.Entities;

/// <summary>
/// Many-to-many link: which <see cref="ServiceCategory"/> options are relevant for a given
/// <see cref="EventType"/> (e.g. a Wedding offers a Bridal Chair; a Corporate Event does not).
/// Fully data-driven — editing the pairs needs no code change. Composite key
/// (EventTypeId, ServiceCategoryId) via Fluent API.
/// </summary>
public class EventTypeServiceCategory
{
    public int EventTypeId { get; set; }
    public EventType EventType { get; set; } = null!;

    public int ServiceCategoryId { get; set; }
    public ServiceCategory ServiceCategory { get; set; } = null!;
}

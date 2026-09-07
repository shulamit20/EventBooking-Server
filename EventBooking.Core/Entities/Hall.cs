namespace EventBooking.Core.Entities;

/// <summary>A specific hall inside a <see cref="Venue"/>.</summary>
public class Hall
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public string? Description { get; set; }

    // Foreign key: Hall * -> 1 Venue
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    // Navigation: Hall 1 -> * HallSlot
    public ICollection<HallSlot> Slots { get; set; } = new List<HallSlot>();
}

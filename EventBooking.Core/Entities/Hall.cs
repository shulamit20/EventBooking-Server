namespace EventBooking.Core.Entities;

/// <summary>A specific hall inside a <see cref="Venue"/>.</summary>
public class Hall
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public string? Description { get; set; }

    // Default per-shift base price, used when generating a month's worth of slots
    // (see IHallSlotService.GenerateAsync). A manually created HallSlot may still override.
    public decimal MorningPrice { get; set; }
    public decimal NoonPrice { get; set; }
    public decimal EveningPrice { get; set; }

    // Foreign key: Hall * -> 1 Venue
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    // Navigation: Hall 1 -> * HallSlot
    public ICollection<HallSlot> Slots { get; set; } = new List<HallSlot>();
}

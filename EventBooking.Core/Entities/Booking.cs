using EventBooking.Core.Enums;

namespace EventBooking.Core.Entities;

/// <summary>A client's reservation of one <see cref="HallSlot"/>, with optional extra services.</summary>
public class Booking
{
    public int Id { get; set; }

    // Foreign key: Booking * -> 1 HallSlot
    public int HallSlotId { get; set; }
    public HallSlot HallSlot { get; set; } = null!;

    // Foreign key: Booking * -> 1 User (the client who made it)
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;
    
    /// <summary>Free text: "Wedding", "BarMitzvah", "Engagement", "ShevaBrachos", "CommunityEvent".</summary>
    public string EventType { get; set; } = null!;

    public string HostName { get; set; } = null!;
    public int GuestCount { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    // Navigation: many-to-many with ExtraService, through the BookingExtraService join entity.
    public ICollection<BookingExtraService> BookingExtraServices { get; set; } = new List<BookingExtraService>();
}

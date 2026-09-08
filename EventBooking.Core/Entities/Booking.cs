using EventBooking.Core.Enums;

namespace EventBooking.Core.Entities;

/// <summary>A customer's reservation of one <see cref="HallSlot"/>, with optional extra services.</summary>
public class Booking
{
    public int Id { get; set; }

    // Foreign key: Booking * -> 1 HallSlot
    public int HallSlotId { get; set; }
    public HallSlot HallSlot { get; set; } = null!;

    // Foreign key: Booking * -> 1 User (the customer who made it)
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    // Foreign key: Booking * -> 1 EventType
    public int EventTypeId { get; set; }
    public EventType EventType { get; set; } = null!;

    // Foreign key: Booking * -> 0..1 CateringMenu (optional)
    public int? CateringMenuId { get; set; }
    public CateringMenu? CateringMenu { get; set; }

    public string HostName { get; set; } = null!;
    public int GuestCount { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    /// <summary>Total price, computed on the server from database prices when the booking changes.</summary>
    public decimal TotalPrice { get; set; }

    // Navigation: many-to-many with ExtraService, through the BookingExtraService join entity.
    public ICollection<BookingExtraService> BookingExtraServices { get; set; } = new List<BookingExtraService>();
}

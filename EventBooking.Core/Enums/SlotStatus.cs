namespace EventBooking.Core.Enums;

/// <summary>Booking state of a single hall slot (the limited resource).</summary>
public enum SlotStatus
{
    Available = 0,
    Booked = 1,
    Blocked = 2
}

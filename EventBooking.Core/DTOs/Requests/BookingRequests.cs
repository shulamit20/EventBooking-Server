using System.ComponentModel.DataAnnotations;
using EventBooking.Core.Enums;

namespace EventBooking.Core.DTOs.Requests;

public class CreateBookingRequest
{
    [Required]
    public int HallSlotId { get; set; }

    [Required, StringLength(100)]
    public string EventType { get; set; } = null!;

    [Required, StringLength(200)]
    public string HostName { get; set; } = null!;

    [Range(1, 5000)]
    public int GuestCount { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    /// <summary>Optional add-ons. May be empty.</summary>
    public List<BookingExtraServiceRequest> ExtraServices { get; set; } = new();
}

public class BookingExtraServiceRequest
{
    [Required]
    public int ExtraServiceId { get; set; }

    [Range(1, 10000)]
    public int Quantity { get; set; } = 1;
}

/// <summary>Manager-only: move a booking between Pending / Confirmed / Cancelled.</summary>
public class UpdateBookingStatusRequest
{
    [Required, EnumDataType(typeof(BookingStatus))]
    public BookingStatus Status { get; set; }
}

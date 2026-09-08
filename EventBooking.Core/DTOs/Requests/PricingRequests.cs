using System.ComponentModel.DataAnnotations;

namespace EventBooking.Core.DTOs.Requests;

/// <summary>
/// A hypothetical selection the Event Builder sends to preview a price. The server computes the
/// total from database prices — the client never sends amounts.
/// </summary>
public class PriceEstimateRequest
{
    [Required] public int HallSlotId { get; set; }

    [Range(1, 5000)] public int GuestCount { get; set; }

    public int? CateringMenuId { get; set; }

    public List<BookingExtraServiceRequest> ExtraServices { get; set; } = new();
}

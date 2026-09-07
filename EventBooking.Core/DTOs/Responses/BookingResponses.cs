namespace EventBooking.Core.DTOs.Responses;

public class BookingResponse
{
    public int Id { get; init; }

    public int HallSlotId { get; init; }
    public string HallName { get; init; } = null!;
    public string VenueName { get; init; } = null!;
    public DateTime Date { get; init; }
    public string Shift { get; init; } = null!;

    public string EventType { get; init; } = null!;
    public string HostName { get; init; } = null!;
    public int GuestCount { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAtUtc { get; init; }
    public string? Notes { get; init; }

    /// <summary>Slot base price + all extra-service line totals.</summary>
    public decimal TotalPrice { get; init; }

    public IReadOnlyList<BookingExtraServiceResponse> ExtraServices { get; init; }
        = Array.Empty<BookingExtraServiceResponse>();
}

public class BookingExtraServiceResponse
{
    public int ExtraServiceId { get; init; }
    public string Name { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal PriceAtBooking { get; init; }
    public decimal LineTotal => Quantity * PriceAtBooking;
}

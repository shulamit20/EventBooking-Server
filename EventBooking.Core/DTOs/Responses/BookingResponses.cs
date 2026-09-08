namespace EventBooking.Core.DTOs.Responses;

public class BookingResponse
{
    public int Id { get; init; }

    public int? HallSlotId { get; init; }
    public string? HallName { get; init; }
    public string? VenueName { get; init; }
    public DateTime? Date { get; init; }
    public string? Shift { get; init; }

    public int EventTypeId { get; init; }
    public string EventTypeName { get; init; } = null!;

    public int? CateringMenuId { get; init; }
    public string? CateringMenuName { get; init; }

    public string HostName { get; init; } = null!;
    public int GuestCount { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAtUtc { get; init; }
    public string? Notes { get; init; }

    /// <summary>Server-computed total: slot base price + all service line totals.</summary>
    public decimal TotalPrice { get; init; }

    public IReadOnlyList<BookingExtraServiceResponse> ExtraServices { get; init; }
        = Array.Empty<BookingExtraServiceResponse>();
}

public class BookingExtraServiceResponse
{
    public int ExtraServiceId { get; init; }
    public string Name { get; init; } = null!;
    public string Category { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal PriceAtBooking { get; init; }
    public decimal LineTotal { get; init; }
}

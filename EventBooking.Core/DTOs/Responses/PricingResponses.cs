namespace EventBooking.Core.DTOs.Responses;

/// <summary>A full, itemised price — every number computed server-side from database prices.</summary>
public class PriceBreakdownResponse
{
    public decimal VenueBase { get; init; }

    public int GuestCount { get; init; }

    public int? CateringMenuId { get; init; }
    public string? CateringMenuName { get; init; }
    public decimal CateringPricePerGuest { get; init; }
    public decimal CateringTotal { get; init; }

    public IReadOnlyList<PriceLineResponse> ServiceLines { get; init; } = Array.Empty<PriceLineResponse>();

    public decimal Total { get; init; }
}

public class PriceLineResponse
{
    public int ExtraServiceId { get; init; }
    public string Name { get; init; } = null!;
    public string Pricing { get; init; } = null!;     // "Flat" or "PerGuest"
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}

public class CateringMenuResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal PricePerGuest { get; init; }
    public bool IsVegetarian { get; init; }
    public bool IsVegan { get; init; }
    public bool IncludesDrinks { get; init; }
    public bool IsActive { get; init; }
    public Guid OwnerUserId { get; init; }
}

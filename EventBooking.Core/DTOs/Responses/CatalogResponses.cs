namespace EventBooking.Core.DTOs.Responses;

public class VenueResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string? ContactPhone { get; init; }
    public string? Description { get; init; }
    public Guid OwnerUserId { get; init; }
}

public class HallResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
    public string? Description { get; init; }
    public int VenueId { get; init; }
    public string VenueName { get; init; } = null!;
}

public class HallSlotResponse
{
    public int Id { get; init; }
    public int HallId { get; init; }
    public string HallName { get; init; } = null!;
    public string VenueName { get; init; } = null!;
    public DateTime Date { get; init; }
    public string Shift { get; init; } = null!;
    public decimal BasePrice { get; init; }
    public string Status { get; init; } = null!;
}

public class ExtraServiceResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Pricing { get; init; } = null!;
    public string? UnitLabel { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; }
    public int ServiceCategoryId { get; init; }
    public string Category { get; init; } = null!;
    public Guid OwnerUserId { get; init; }
}

// ---- lookups (for populating client dropdowns) ----

public class EventTypeResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public IReadOnlyList<int> ServiceCategoryIds { get; init; } = Array.Empty<int>();
}

public class ServiceCategoryResponse
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
}

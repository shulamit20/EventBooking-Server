namespace EventBooking.Core.DTOs.Responses;

public class VenueResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string? ContactPhone { get; init; }
    public string? Description { get; init; }
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
    public string? UnitLabel { get; init; }
}

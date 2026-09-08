using System.ComponentModel.DataAnnotations;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.Enums;

namespace EventBooking.Core.DTOs.Requests;

// ---- Venue (full CRUD lives on this entity) ----

public class CreateVenueRequest
{
    [Required, StringLength(200)] public string Name { get; set; } = null!;
    [Required, StringLength(100)] public string City { get; set; } = null!;
    [Required, StringLength(300)] public string Address { get; set; } = null!;
    [StringLength(30)] public string? ContactPhone { get; set; }
    [StringLength(1000)] public string? Description { get; set; }
}

public class UpdateVenueRequest : CreateVenueRequest { }

// ---- Hall ----

public class CreateHallRequest
{
    [Required] public int VenueId { get; set; }
    [Required, StringLength(200)] public string Name { get; set; } = null!;
    [Range(1, 100_000)] public int Capacity { get; set; }
    [StringLength(1000)] public string? Description { get; set; }
}

// ---- Hall slot (the limited resource) ----

public class CreateHallSlotRequest
{
    [Required] public int HallId { get; set; }

    [Required] public DateTime Date { get; set; }

    [Required, EnumDataType(typeof(ShiftType))]
    public ShiftType Shift { get; set; }

    [Range(0, 1_000_000)] public decimal BasePrice { get; set; }
}

/// <summary>Filter + sort + paging for browsing slots. Bound from the query string.</summary>
public class HallSlotQuery : PageQuery
{
    public int? HallId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public SlotStatus? Status { get; set; }

    /// <summary>"date" (default) or "price".</summary>
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
}

// ---- Extra service ----

public class CreateExtraServiceRequest
{
    [Required, StringLength(200)] public string Name { get; set; } = null!;
    [StringLength(1000)] public string? Description { get; set; }
    [Range(0, 1_000_000)] public decimal Price { get; set; }

    [Required] public int ServiceCategoryId { get; set; }

    [EnumDataType(typeof(PricingModel))]
    public PricingModel Pricing { get; set; } = PricingModel.Flat;

    [StringLength(50)] public string? UnitLabel { get; set; }
    [StringLength(500)] public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateExtraServiceRequest : CreateExtraServiceRequest { }

using EventBooking.Core.Enums;

namespace EventBooking.Core.Entities;

/// <summary>
/// An add-on a customer can attach to a booking — catering, table design, bridal chair,
/// photography, DJ, and so on. The <see cref="ServiceCategory"/> makes the list extensible;
/// <see cref="Pricing"/> decides how <see cref="Price"/> becomes a line total.
/// </summary>
public class ExtraService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public decimal Price { get; set; }

    /// <summary>Flat (× quantity) or per guest (× guest count).</summary>
    public PricingModel Pricing { get; set; } = PricingModel.Flat;

    /// <summary>How the price is counted, e.g. "per plate", "per event".</summary>
    public string? UnitLabel { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>Hidden from customers when false; existing bookings keep it.</summary>
    public bool IsActive { get; set; } = true;

    // Foreign key: ExtraService * -> 1 ServiceCategory
    public int ServiceCategoryId { get; set; }
    public ServiceCategory ServiceCategory { get; set; } = null!;

    // Foreign key: ExtraService * -> 1 User (the Manager who owns / offers it)
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    // Navigation: many-to-many with Booking, through the BookingExtraService join entity.
    public ICollection<BookingExtraService> BookingExtraServices { get; set; } = new List<BookingExtraService>();
}

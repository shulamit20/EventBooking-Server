namespace EventBooking.Core.Entities;

/// <summary>
/// A catering package a customer can add to their event. Its own entity (not an
/// <see cref="ExtraService"/>) because it carries menu-specific data and its price scales with
/// the guest count — the server multiplies <see cref="PricePerGuest"/> by the booking's
/// <c>GuestCount</c>.
/// </summary>
public class CateringMenu
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public decimal PricePerGuest { get; set; }

    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IncludesDrinks { get; set; }

    public bool IsActive { get; set; } = true;

    // Foreign key: CateringMenu * -> 1 User (the Manager who offers it)
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;

namespace EventBooking.Core.DTOs.Requests;

public class CreateCateringMenuRequest
{
    [Required, StringLength(200)] public string Name { get; set; } = null!;
    [StringLength(1000)] public string? Description { get; set; }
    [Range(0, 100_000)] public decimal PricePerGuest { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IncludesDrinks { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCateringMenuRequest : CreateCateringMenuRequest { }

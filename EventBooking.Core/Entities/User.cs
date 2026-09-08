using EventBooking.Core.Enums;

namespace EventBooking.Core.Entities;

/// <summary>A login account. Used for JWT authentication and role-based authorization.</summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = null!;

    /// <summary>Hashed password only. Never store the plain text.</summary>
    public string PasswordHash { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.Customer;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();          // as customer
    public ICollection<Venue> OwnedVenues { get; set; } = new List<Venue>();           // as manager
    public ICollection<ExtraService> OwnedServices { get; set; } = new List<ExtraService>(); // as manager
    public ICollection<CateringMenu> OwnedCateringMenus { get; set; } = new List<CateringMenu>(); // as manager
}

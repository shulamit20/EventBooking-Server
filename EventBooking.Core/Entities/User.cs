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

    public UserRole Role { get; set; } = UserRole.Client;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation: User 1 -> * Booking
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

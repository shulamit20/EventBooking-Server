namespace EventBooking.Core.Entities;

/// <summary>A location / complex that contains one or more halls.</summary>
public class Venue
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? ContactPhone { get; set; }
    public string? Description { get; set; }

    // Foreign key: Venue * -> 1 User (the Manager who owns it and may edit its halls / slots)
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    // Navigation: Venue 1 -> * Hall
    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
}

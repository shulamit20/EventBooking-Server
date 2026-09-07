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

    // Navigation: Venue 1 -> * Hall
    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
}

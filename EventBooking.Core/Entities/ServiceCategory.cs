namespace EventBooking.Core.Entities;

/// <summary>
/// A category of extra service (Catering, TableDesign, BridalChair, ...). A lookup table so the
/// list is extensible without a redeploy. <see cref="Code"/> is a stable machine-readable key
/// (business logic that must recognise a category — e.g. showing the Bridal Chair step for
/// weddings — matches on <see cref="Code"/>, never on the display <see cref="Name"/>).
/// </summary>
public class ServiceCategory
{
    public int Id { get; set; }

    /// <summary>Stable key, e.g. "Catering", "BridalChair". Unique.</summary>
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ExtraService> Services { get; set; } = new List<ExtraService>();
    public ICollection<EventTypeServiceCategory> EventTypes { get; set; } = new List<EventTypeServiceCategory>();
}

using EventBooking.Core.Entities;

namespace EventBooking.Core.Interfaces.Repositories;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>One page of venues, ordered by name. Total is the unpaged count.</summary>
    Task<(IReadOnlyList<Venue> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    Task AddAsync(Venue venue, CancellationToken ct = default);
    void Remove(Venue venue);
}

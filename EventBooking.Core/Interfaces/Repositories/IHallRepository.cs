using EventBooking.Core.Entities;

namespace EventBooking.Core.Interfaces.Repositories;

public interface IHallRepository
{
    /// <summary>Loads the hall together with its venue (for building the response).</summary>
    Task<Hall?> GetByIdWithVenueAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Hall>> GetByVenueAsync(int venueId, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    Task AddAsync(Hall hall, CancellationToken ct = default);
}

using EventBooking.Core.Entities;

namespace EventBooking.Core.Interfaces.Repositories;

public interface IExtraServiceRepository
{
    Task<IReadOnlyList<ExtraService>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Loads exactly the services requested on a booking (single query, no N+1).</summary>
    Task<IReadOnlyList<ExtraService>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

    Task AddAsync(ExtraService service, CancellationToken ct = default);
}

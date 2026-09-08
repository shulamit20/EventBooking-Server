using EventBooking.Core.Entities;

namespace EventBooking.Core.Interfaces.Repositories;

public interface ICateringMenuRepository
{
    /// <summary>All active menus, for customers to browse.</summary>
    Task<IReadOnlyList<CateringMenu>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Every menu owned by one manager (active or not).</summary>
    Task<IReadOnlyList<CateringMenu>> GetByOwnerAsync(Guid ownerUserId, CancellationToken ct = default);

    /// <summary>Tracked — used by update / delete.</summary>
    Task<CateringMenu?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(CateringMenu menu, CancellationToken ct = default);
    void Remove(CateringMenu menu);
}

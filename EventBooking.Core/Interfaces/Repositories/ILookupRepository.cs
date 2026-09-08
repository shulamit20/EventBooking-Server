using EventBooking.Core.Entities;

namespace EventBooking.Core.Interfaces.Repositories;

/// <summary>Read access to the lookup tables (<see cref="EventType"/>, <see cref="ServiceCategory"/>).</summary>
public interface ILookupRepository
{
    Task<IReadOnlyList<EventType>> GetEventTypesAsync(CancellationToken ct = default);
    Task<bool> EventTypeExistsAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<ServiceCategory>> GetServiceCategoriesAsync(CancellationToken ct = default);
}

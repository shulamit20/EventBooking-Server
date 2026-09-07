using EventBooking.Core.Entities;
using EventBooking.Core.Enums;

namespace EventBooking.Core.Interfaces.Repositories;

public interface IHallSlotRepository
{
    /// <summary>Tracked load — use when the slot is about to be modified (booking flow).</summary>
    Task<HallSlot?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Read-only load with hall + venue, for building a response.</summary>
    Task<HallSlot?> GetByIdWithHallAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// One page of slots after filtering and sorting, all done in the database query.
    /// </summary>
    Task<(IReadOnlyList<HallSlot> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        int? hallId,
        DateTime? fromDate,
        DateTime? toDate,
        SlotStatus? status,
        string? sortBy,
        bool descending,
        CancellationToken ct = default);

    Task AddAsync(HallSlot slot, CancellationToken ct = default);
}

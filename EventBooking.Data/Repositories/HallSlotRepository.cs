using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class HallSlotRepository : IHallSlotRepository
{
    private readonly AppDbContext _context;

    public HallSlotRepository(AppDbContext context) => _context = context;

    /// <summary>Tracked: the booking flow reads the slot and then changes its Status.</summary>
    public Task<HallSlot?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.HallSlots.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<HallSlot?> GetByIdWithHallAsync(int id, CancellationToken ct = default) =>
        _context.HallSlots.AsNoTracking()
            .Include(s => s.Hall).ThenInclude(h => h.Venue)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<(IReadOnlyList<HallSlot> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        int? hallId,
        DateTime? fromDate,
        DateTime? toDate,
        SlotStatus? status,
        string? sortBy,
        bool descending,
        CancellationToken ct = default)
    {
        IQueryable<HallSlot> query = _context.HallSlots.AsNoTracking()
            .Include(s => s.Hall).ThenInclude(h => h.Venue);

        if (hallId is not null)
            query = query.Where(s => s.HallId == hallId.Value);

        if (fromDate is not null)
        {
            var from = fromDate.Value.Date;
            query = query.Where(s => s.Date >= from);
        }

        if (toDate is not null)
        {
            var to = toDate.Value.Date;
            query = query.Where(s => s.Date <= to);
        }

        if (status is not null)
            query = query.Where(s => s.Status == status.Value);

        bool byPrice = string.Equals(sortBy, "price", StringComparison.OrdinalIgnoreCase);
        query = (byPrice, descending) switch
        {
            (true, false) => query.OrderBy(s => s.BasePrice).ThenBy(s => s.Id),
            (true, true) => query.OrderByDescending(s => s.BasePrice).ThenBy(s => s.Id),
            (false, true) => query.OrderByDescending(s => s.Date).ThenBy(s => s.Id),
            _ => query.OrderBy(s => s.Date).ThenBy(s => s.Id),
        };

        var total = await query.CountAsync(ct);

        // Skip/Take go into the SQL query - real pagination, not in-memory slicing.
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(HallSlot slot, CancellationToken ct = default) =>
        await _context.HallSlots.AddAsync(slot, ct);
}

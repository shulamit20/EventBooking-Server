using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context) => _context = context;

    /// <summary>Tracked, with line items: used by Cancel / SetStatus which mutate the booking.</summary>
    public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Bookings
            .Include(b => b.BookingExtraServices)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<Booking?> GetDetailedByIdAsync(int id, CancellationToken ct = default) =>
        DetailedQuery().FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<bool> HasActiveBookingForSlotAsync(int hallSlotId, CancellationToken ct = default) =>
        _context.Bookings.AnyAsync(
            b => b.HallSlotId == hallSlotId && b.Status != BookingStatus.Cancelled, ct);

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedByOwnerAsync(
        Guid ownerUserId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DetailedQuery()
            .Where(b => b.OwnerUserId == ownerUserId)
            .OrderByDescending(b => b.CreatedAtUtc).ThenByDescending(b => b.Id);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, BookingStatus? status, CancellationToken ct = default)
    {
        var query = DetailedQuery();

        if (status is not null)
            query = query.Where(b => b.Status == status.Value);

        query = query.OrderByDescending(b => b.CreatedAtUtc).ThenByDescending(b => b.Id);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Booking booking, CancellationToken ct = default) =>
        await _context.Bookings.AddAsync(booking, ct);

    /// <summary>
    /// Read-only projection base: booking + slot -> hall -> venue + extra services, in a
    /// split query. Fixed number of SQL round-trips regardless of row count (no N+1).
    /// </summary>
    private IQueryable<Booking> DetailedQuery() =>
        _context.Bookings.AsNoTracking()
            .Include(b => b.EventType)
            .Include(b => b.HallSlot!).ThenInclude(s => s.Hall).ThenInclude(h => h.Venue)
            .Include(b => b.BookingExtraServices).ThenInclude(x => x.ExtraService).ThenInclude(e => e.ServiceCategory)
            .AsSplitQuery();
}

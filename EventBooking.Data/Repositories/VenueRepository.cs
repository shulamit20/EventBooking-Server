using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context) => _context = context;

    /// <summary>Tracked on purpose: the same load is reused by Update / Delete in the service.</summary>
    public Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Venues.FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<(IReadOnlyList<Venue> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Venues.AsNoTracking().OrderBy(v => v.Name).ThenBy(v => v.Id);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Venue venue, CancellationToken ct = default) =>
        await _context.Venues.AddAsync(venue, ct);

    public void Remove(Venue venue) => _context.Venues.Remove(venue);
}

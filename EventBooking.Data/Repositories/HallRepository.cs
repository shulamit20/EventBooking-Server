using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class HallRepository : IHallRepository
{
    private readonly AppDbContext _context;

    public HallRepository(AppDbContext context) => _context = context;

    public Task<Hall?> GetByIdWithVenueAsync(int id, CancellationToken ct = default) =>
        _context.Halls.AsNoTracking()
            .Include(h => h.Venue)
            .FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<IReadOnlyList<Hall>> GetByVenueAsync(int venueId, CancellationToken ct = default) =>
        await _context.Halls.AsNoTracking()
            .Include(h => h.Venue)
            .Where(h => h.VenueId == venueId)
            .OrderBy(h => h.Name)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        _context.Halls.AnyAsync(h => h.Id == id, ct);

    public async Task AddAsync(Hall hall, CancellationToken ct = default) =>
        await _context.Halls.AddAsync(hall, ct);
}

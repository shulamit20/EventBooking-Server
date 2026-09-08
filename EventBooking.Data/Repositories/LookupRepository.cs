using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class LookupRepository : ILookupRepository
{
    private readonly AppDbContext _context;

    public LookupRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<EventType>> GetEventTypesAsync(CancellationToken ct = default) =>
        await _context.EventTypes.AsNoTracking()
            .Where(t => t.IsActive)
            .Include(t => t.ServiceCategories)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);

    public Task<bool> EventTypeExistsAsync(int id, CancellationToken ct = default) =>
        _context.EventTypes.AnyAsync(t => t.Id == id && t.IsActive, ct);

    public async Task<IReadOnlyList<ServiceCategory>> GetServiceCategoriesAsync(CancellationToken ct = default) =>
        await _context.ServiceCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);
}

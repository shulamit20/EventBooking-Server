using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class CateringMenuRepository : ICateringMenuRepository
{
    private readonly AppDbContext _context;

    public CateringMenuRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<CateringMenu>> GetActiveAsync(CancellationToken ct = default) =>
        await _context.CateringMenus.AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.PricePerGuest)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CateringMenu>> GetByOwnerAsync(Guid ownerUserId, CancellationToken ct = default) =>
        await _context.CateringMenus.AsNoTracking()
            .Where(m => m.OwnerUserId == ownerUserId)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

    public Task<CateringMenu?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.CateringMenus.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddAsync(CateringMenu menu, CancellationToken ct = default) =>
        await _context.CateringMenus.AddAsync(menu, ct);

    public void Remove(CateringMenu menu) => _context.CateringMenus.Remove(menu);
}

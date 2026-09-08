using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class ExtraServiceRepository : IExtraServiceRepository
{
    private readonly AppDbContext _context;

    public ExtraServiceRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<ExtraService>> GetAllAsync(CancellationToken ct = default) =>
        await _context.ExtraServices.AsNoTracking()
            .Include(e => e.ServiceCategory)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ExtraService>> GetByIdsAsync(
        IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();

        // One IN (...) query for all requested services - the booking service needs their prices.
        return await _context.ExtraServices.AsNoTracking()
            .Include(e => e.ServiceCategory)
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(ExtraService service, CancellationToken ct = default) =>
        await _context.ExtraServices.AddAsync(service, ct);
}

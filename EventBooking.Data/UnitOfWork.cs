using EventBooking.Core.Interfaces;

namespace EventBooking.Data;

/// <summary>
/// The single commit point over <see cref="AppDbContext"/>. Repositories in this scope share the
/// same context instance, so one call here persists every staged change in one transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}

namespace EventBooking.Core.Interfaces;

/// <summary>
/// Commits every change tracked across the repositories in the current scope, in one transaction.
/// Repositories only stage changes (Add / mutate tracked entities); this is the single save point.
/// Implemented by the Data layer over the same <c>DbContext</c> the repositories use.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes.
    /// May throw <c>DbUpdateConcurrencyException</c> when an optimistic-concurrency token no longer
    /// matches — the caller (booking service) catches it and returns a Conflict result.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

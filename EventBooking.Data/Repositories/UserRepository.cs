using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    // Read-only lookups -> AsNoTracking. Nothing in the current surface mutates a User after creation.
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        _context.Users.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _context.Users.AddAsync(user, ct);
}

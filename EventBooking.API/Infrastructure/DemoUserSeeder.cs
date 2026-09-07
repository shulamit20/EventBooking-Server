using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Security;

namespace EventBooking.API.Infrastructure;

/// <summary>
/// Inserts the two demo accounts (one per role) if they are not in the database yet.
/// Runs on startup. These users cannot be seeded inside a migration because seeding
/// there cannot call the password hasher. Credentials are documented in the README.
/// </summary>
public static class DemoUserSeeder
{
    public const string ClientEmail = "client@eventbooking.local";
    public const string ManagerEmail = "manager@eventbooking.local";
    public const string DemoPassword = "Passw0rd!";

    public static async Task SeedDemoUsersAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var added = false;
        added |= await EnsureUserAsync(users, hasher, ClientEmail, "Demo Client", UserRole.Client);
        added |= await EnsureUserAsync(users, hasher, ManagerEmail, "Demo Manager", UserRole.Manager);

        if (added)
            await uow.SaveChangesAsync();
    }

    private static async Task<bool> EnsureUserAsync(
        IUserRepository users, IPasswordHasher hasher,
        string email, string displayName, UserRole role)
    {
        if (await users.EmailExistsAsync(email))
            return false;

        await users.AddAsync(new User
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = hasher.Hash(DemoPassword),
            Role = role
        });
        return true;
    }
}

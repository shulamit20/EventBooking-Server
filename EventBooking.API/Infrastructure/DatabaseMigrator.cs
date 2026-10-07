using EventBooking.Data;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.API.Infrastructure;

/// <summary>
/// Applies pending EF Core migrations at startup. Needed on hosts like Render, where there is
/// no separate step to run `dotnet ef database update` against the deployed database.
/// </summary>
public static class DatabaseMigrator
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}

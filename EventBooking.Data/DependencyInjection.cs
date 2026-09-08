using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Data;

/// <summary>
/// Registers everything the Data layer owns. Called once from Program.cs.
/// API references Data only for this call — never for its concrete types.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:Default' is missing. " +
                "Set it with: dotnet user-secrets set \"ConnectionStrings:Default\" \"...\" --project EventBooking.API");

        // Scoped by default: one AppDbContext per HTTP request.
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        // All Scoped: they share the request's AppDbContext.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<IHallSlotRepository, HallSlotRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IExtraServiceRepository, ExtraServiceRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<ICateringMenuRepository, CateringMenuRepository>();

        return services;
    }
}

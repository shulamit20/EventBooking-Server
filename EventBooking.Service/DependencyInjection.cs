using EventBooking.Core.Interfaces.Security;
using EventBooking.Core.Interfaces.Services;
using EventBooking.Service.Mapping;
using EventBooking.Service.Security;
using EventBooking.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Service;

/// <summary>
/// Registers everything the Service layer owns. Called once from Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // AutoMapper: scans this assembly for Profile classes (Mapping/ folder).
        services.AddAutoMapper(typeof(CatalogMappingProfile).Assembly);

        // Business services - Scoped: they use the request-scoped repositories / DbContext.
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<IHallSlotService, HallSlotService>();
        services.AddScoped<IExtraServiceService, ExtraServiceService>();
        services.AddScoped<IBookingService, BookingService>();

        // Auth. JwtTokenService consumes IOptions<JwtOptions>; the composition root (Program.cs)
        // owns binding + validation of that section.
        // Hasher and token service are stateless -> Singleton. AuthService uses scoped repos -> Scoped.
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}

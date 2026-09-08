using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

/// <summary>Serves the lookup lists the client needs (event types, service categories).</summary>
public interface ILookupService
{
    Task<IReadOnlyList<EventTypeResponse>> GetEventTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ServiceCategoryResponse>> GetServiceCategoriesAsync(CancellationToken ct = default);
}

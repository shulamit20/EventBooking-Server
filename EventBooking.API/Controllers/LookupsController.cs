using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

/// <summary>
/// Read-only lists the client needs to build forms: event types (with the service categories
/// each one offers) and the service-category catalogue. Public.
/// </summary>
[Route("api")]
[AllowAnonymous]
public class LookupsController : ApiControllerBase
{
    private readonly ILookupService _lookups;

    public LookupsController(ILookupService lookups) => _lookups = lookups;

    [HttpGet("event-types")]
    public async Task<ActionResult<IReadOnlyList<EventTypeResponse>>> GetEventTypes(CancellationToken ct) =>
        Ok(await _lookups.GetEventTypesAsync(ct));

    [HttpGet("service-categories")]
    public async Task<ActionResult<IReadOnlyList<ServiceCategoryResponse>>> GetServiceCategories(CancellationToken ct) =>
        Ok(await _lookups.GetServiceCategoriesAsync(ct));
}

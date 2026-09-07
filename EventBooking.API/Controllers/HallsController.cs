using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EventBooking.API.Controllers;

[Route("api/halls")]
public class HallsController : ApiControllerBase
{
    private readonly IHallService _halls;

    public HallsController(IHallService halls) => _halls = halls;

    /// <summary>All halls of one venue. 404 if the venue does not exist.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<HallResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HallResponse>>> GetByVenue(
        [FromQuery, BindRequired] int venueId, CancellationToken ct) =>
        ToResponse(await _halls.GetByVenueAsync(venueId, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HallResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HallResponse>> GetById([FromRoute] int id, CancellationToken ct) =>
        ToResponse(await _halls.GetByIdAsync(id, ct));

    /// <summary>Adds a hall to a venue. 400 if the venue is missing or the name is taken there.</summary>
    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(HallResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HallResponse>> Create([FromBody] CreateHallRequest request, CancellationToken ct)
    {
        var result = await _halls.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ErrorResult(result);
    }
}

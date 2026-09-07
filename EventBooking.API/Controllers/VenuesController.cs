using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[Route("api/venues")]
public class VenuesController : ApiControllerBase
{
    private readonly IVenueService _venues;

    public VenuesController(IVenueService venues) => _venues = venues;

    /// <summary>Browse venues, one page at a time.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<VenueResponse>>> GetAll([FromQuery] PageQuery query, CancellationToken ct) =>
        Ok(await _venues.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VenueResponse>> GetById([FromRoute] int id, CancellationToken ct) =>
        ToResponse(await _venues.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VenueResponse>> Create([FromBody] CreateVenueRequest request, CancellationToken ct)
    {
        var result = await _venues.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ErrorResult(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VenueResponse>> Update(
        [FromRoute] int id, [FromBody] UpdateVenueRequest request, CancellationToken ct) =>
        ToResponse(await _venues.UpdateAsync(id, request, ct));

    /// <summary>Deletes a venue. 409 if it still has halls (the FK is Restrict).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct) =>
        ToResponse(await _venues.DeleteAsync(id, ct));
}

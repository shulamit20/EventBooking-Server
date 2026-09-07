using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

/// <summary>The limited resource. Anyone may browse; only a Manager creates slots.</summary>
[Route("api/hall-slots")]
public class HallSlotsController : ApiControllerBase
{
    private readonly IHallSlotService _slots;

    public HallSlotsController(IHallSlotService slots) => _slots = slots;

    /// <summary>Browse slots with filter (hall / date range / status), sort and paging — all applied in SQL.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<HallSlotResponse>>> GetAll(
        [FromQuery] HallSlotQuery query, CancellationToken ct) =>
        Ok(await _slots.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HallSlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HallSlotResponse>> GetById([FromRoute] int id, CancellationToken ct) =>
        ToResponse(await _slots.GetByIdAsync(id, ct));

    /// <summary>Creates a slot. 400 if the hall is missing or (hall, date, shift) already exists.</summary>
    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(HallSlotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HallSlotResponse>> Create([FromBody] CreateHallSlotRequest request, CancellationToken ct)
    {
        var result = await _slots.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ErrorResult(result);
    }
}

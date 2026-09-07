using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

/// <summary>Every endpoint here needs a logged-in user; roles narrow it further per action.</summary>
[Route("api/bookings")]
[Authorize]
public class BookingsController : ApiControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings) => _bookings = bookings;

    /// <summary>
    /// The competed operation: a Client books a slot. 409 if the slot was taken in the meantime
    /// (optimistic concurrency on <c>HallSlot.Version</c>).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookings.CreateAsync(request, CurrentUserId, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ErrorResult(result);
    }

    /// <summary>The caller's own bookings, paged.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetMine(
        [FromQuery] PageQuery query, CancellationToken ct) =>
        Ok(await _bookings.GetMineAsync(CurrentUserId, query, ct));

    /// <summary>One booking. A Client may only read their own; a Manager may read any (403 otherwise).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetById([FromRoute] int id, CancellationToken ct) =>
        ToResponse(await _bookings.GetByIdAsync(id, CurrentUserId, IsManager, ct));

    /// <summary>Manager view: every booking, optionally filtered by status, paged.</summary>
    [HttpGet]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetAll(
        [FromQuery] PageQuery query, [FromQuery] BookingStatus? status, CancellationToken ct) =>
        Ok(await _bookings.GetAllAsync(query, status, ct));

    /// <summary>A Client cancels their own booking; the slot is released. 204 on success.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Cancel([FromRoute] int id, CancellationToken ct)
    {
        var result = await _bookings.CancelAsync(id, CurrentUserId, ct);
        return result.IsSuccess ? NoContent() : ErrorResult(result);
    }

    /// <summary>Manager moves a booking to Confirmed / Cancelled / Pending; the slot follows.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> SetStatus(
        [FromRoute] int id, [FromBody] UpdateBookingStatusRequest request, CancellationToken ct) =>
        ToResponse(await _bookings.SetStatusAsync(id, request.Status, ct));
}

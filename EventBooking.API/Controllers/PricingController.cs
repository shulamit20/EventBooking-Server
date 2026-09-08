using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[Route("api/pricing")]
public class PricingController : ApiControllerBase
{
    private readonly IPriceCalculationService _pricing;

    public PricingController(IPriceCalculationService pricing) => _pricing = pricing;

    /// <summary>
    /// Preview the itemised price of a selection (slot + guests + catering + services) before
    /// booking. Every amount is computed from the database — the client sends no amounts.
    /// </summary>
    [HttpPost("estimate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PriceBreakdownResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceBreakdownResponse>> Estimate(
        [FromBody] PriceEstimateRequest request, CancellationToken ct) =>
        ToResponse(await _pricing.CalculateAsync(
            request.HallSlotId, request.GuestCount, request.CateringMenuId, request.ExtraServices, ct));
}

using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[Route("api/extra-services")]
public class ExtraServicesController : ApiControllerBase
{
    private readonly IExtraServiceService _extras;

    public ExtraServicesController(IExtraServiceService extras) => _extras = extras;

    /// <summary>The full catalog of add-ons a booking can include.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ExtraServiceResponse>>> GetAll(CancellationToken ct) =>
        Ok(await _extras.GetAllAsync(ct));

    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(ExtraServiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExtraServiceResponse>> Create(
        [FromBody] CreateExtraServiceRequest request, CancellationToken ct)
    {
        var result = await _extras.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), routeValues: null, value: result.Value)
            : ErrorResult(result);
    }
}

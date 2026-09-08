using EventBooking.API.Infrastructure;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

[Route("api/catering-menus")]
public class CateringController : ApiControllerBase
{
    private readonly ICateringMenuService _menus;

    public CateringController(ICateringMenuService menus) => _menus = menus;

    /// <summary>Active catering packages a customer can add to an event.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CateringMenuResponse>>> GetActive(CancellationToken ct) =>
        Ok(await _menus.GetActiveAsync(ct));

    /// <summary>The calling manager's own menus (active or not).</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<IReadOnlyList<CateringMenuResponse>>> GetMine(CancellationToken ct) =>
        Ok(await _menus.GetMineAsync(CurrentUserId, ct));

    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(CateringMenuResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CateringMenuResponse>> Create(
        [FromBody] CreateCateringMenuRequest request, CancellationToken ct)
    {
        var result = await _menus.CreateAsync(request, CurrentUserId, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMine), null, result.Value)
            : ErrorResult(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(CateringMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CateringMenuResponse>> Update(
        [FromRoute] int id, [FromBody] UpdateCateringMenuRequest request, CancellationToken ct) =>
        ToResponse(await _menus.UpdateAsync(id, request, CurrentUserId, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct) =>
        ToResponse(await _menus.DeleteAsync(id, CurrentUserId, ct));
}

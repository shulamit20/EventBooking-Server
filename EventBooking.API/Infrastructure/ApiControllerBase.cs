using System.Security.Claims;
using EventBooking.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Infrastructure;

/// <summary>
/// Shared base for every API controller.
/// <para>
/// <see cref="ApiControllerAttribute"/> turns on the MVC conventions the brief asks for:
/// attribute routing is required, complex parameters bind from the body by default, and an
/// invalid <see cref="ControllerBase.ModelState"/> (failed Data Annotations) is turned into an
/// automatic <c>400</c> with a <see cref="ValidationProblemDetails"/> body — that IS the
/// "ModelState check", so no controller repeats it by hand.
/// </para>
/// <para>
/// One place also translates a service <see cref="Result"/> into the matching HTTP status code,
/// so controllers stay thin and every endpoint answers 4xx the same way.
/// </para>
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// The authenticated user's id. JWT bearer maps the token's <c>sub</c> claim to
    /// <see cref="ClaimTypes.NameIdentifier"/>; we accept either name.
    /// </summary>
    protected Guid CurrentUserId =>
        Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("The token carries no user id claim."));

    /// <summary>True when the caller holds the Manager role.</summary>
    protected bool IsManager => User.IsInRole("Manager");

    /// <summary>Value result -> 200 with the value, or the error status.</summary>
    protected ActionResult<T> ToResponse<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : ErrorResult(result);

    /// <summary>Payload-less result -> 204 on success, or the error status.</summary>
    protected ActionResult ToResponse(Result result) =>
        result.IsSuccess ? NoContent() : ErrorResult(result);

    /// <summary>Turns a failed result into a <see cref="ProblemDetails"/> response with the right code.</summary>
    protected ObjectResult ErrorResult(Result result)
    {
        var statusCode = result.Status switch
        {
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            ResultStatus.Invalid => StatusCodes.Status400BadRequest,
            ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
            ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
            ResultStatus.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(detail: result.Error, statusCode: statusCode);
    }
}

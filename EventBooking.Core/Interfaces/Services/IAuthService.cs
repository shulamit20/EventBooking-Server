using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

public interface IAuthService
{
    /// <summary>Creates a Client account and returns a token. Conflict if the email is taken.</summary>
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>Verifies credentials and returns a token. Unauthorized on a bad email/password.</summary>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

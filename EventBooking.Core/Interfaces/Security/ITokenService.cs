using EventBooking.Core.Entities;

namespace EventBooking.Core.Interfaces.Security;

/// <summary>Issues signed JWT access tokens for authenticated users.</summary>
public interface ITokenService
{
    /// <summary>Builds a signed token for the user, together with its UTC expiry time.</summary>
    (string Token, DateTime ExpiresAtUtc) Generate(User user);
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventBooking.Core.Configuration;
using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventBooking.Service.Security;

/// <summary>
/// Builds signed JWTs (HMAC-SHA256) from the "Jwt" configuration section.
/// The API validates these tokens with the same Issuer / Audience / Key.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public (string Token, DateTime ExpiresAtUtc) Generate(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),   // enables [Authorize(Roles = "...")]
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}

using System.Text;
using EventBooking.Core.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventBooking.API.Infrastructure;

/// <summary>
/// Fills in <see cref="JwtBearerOptions"/> from the strongly-typed <see cref="JwtOptions"/>,
/// so incoming tokens are validated against the exact Issuer / Audience / Key the Service layer
/// signs with. Registered with <c>services.ConfigureOptions&lt;ConfigureJwtBearerOptions&gt;()</c>,
/// which is why <c>AddJwtBearer()</c> in Program.cs needs no inline lambda.
/// </summary>
public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwt;

    public ConfigureJwtBearerOptions(IOptions<JwtOptions> jwt) => _jwt = jwt.Value;

    // The bearer scheme is named; both overloads point at the same setup.
    public void Configure(string? name, JwtBearerOptions options) => Configure(options);

    public void Configure(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }
}

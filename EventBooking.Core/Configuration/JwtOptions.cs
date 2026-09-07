using System.ComponentModel.DataAnnotations;

namespace EventBooking.Core.Configuration;

/// <summary>
/// JWT settings, bound from the "Jwt" configuration section and validated on startup.
/// The non-secret parts (<see cref="Issuer"/>, <see cref="Audience"/>, <see cref="ExpiryMinutes"/>)
/// live in appsettings; <see cref="Key"/> must come from User Secrets / environment, never the repo.
/// </summary>
public class JwtOptions
{
    /// <summary>Configuration section these options bind from.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Who issued the token (this API).</summary>
    [Required]
    public string Issuer { get; set; } = null!;

    /// <summary>Who the token is intended for (our client app).</summary>
    [Required]
    public string Audience { get; set; } = null!;

    /// <summary>Symmetric signing key. Secret — set via User Secrets, minimum 32 bytes for HMAC-SHA256.</summary>
    [Required, MinLength(32)]
    public string Key { get; set; } = null!;

    /// <summary>How long a freshly issued token stays valid.</summary>
    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}

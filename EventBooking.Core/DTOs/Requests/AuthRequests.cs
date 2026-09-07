using System.ComponentModel.DataAnnotations;

namespace EventBooking.Core.DTOs.Requests;

public class RegisterRequest
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = null!;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;

    [Required, StringLength(200)]
    public string DisplayName { get; set; } = null!;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

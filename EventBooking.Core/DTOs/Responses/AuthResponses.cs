namespace EventBooking.Core.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; init; } = null!;
    public DateTime ExpiresAtUtc { get; init; }
    public UserResponse User { get; init; } = null!;
}

public class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string Role { get; init; } = null!;
}

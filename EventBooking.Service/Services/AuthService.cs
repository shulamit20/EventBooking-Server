using AutoMapper;
using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Security;
using EventBooking.Core.Interfaces.Services;

namespace EventBooking.Service.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository users,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        ITokenService tokens,
        IMapper mapper)
    {
        _users = users;
        _uow = uow;
        _hasher = hasher;
        _tokens = tokens;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = Normalize(request.Email);

        if (await _users.EmailExistsAsync(email, ct))
            return Result<AuthResponse>.Conflict($"Email '{email}' is already registered.");

        var user = new User
        {
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            Role = UserRole.Client   // self-registration is always a Client; Managers are seeded
        };

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<AuthResponse>.Ok(BuildResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(Normalize(request.Email), ct);

        // Same message whether the email is unknown or the password is wrong — no account enumeration.
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Unauthorized("Invalid email or password.");

        return Result<AuthResponse>.Ok(BuildResponse(user));
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAtUtc) = _tokens.Generate(user);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = _mapper.Map<UserResponse>(user)
        };
    }
}

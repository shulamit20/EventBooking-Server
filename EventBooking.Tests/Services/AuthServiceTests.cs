using AutoMapper;
using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Security;
using EventBooking.Service.Services;
using Moq;

namespace EventBooking.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IMapper> _mapper = new();

    private AuthService CreateSut() =>
        new(_users.Object, _uow.Object, _hasher.Object, _tokens.Object, _mapper.Object);

    public AuthServiceTests()
    {
        _tokens.Setup(t => t.Generate(It.IsAny<User>()))
            .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));
        _mapper.Setup(m => m.Map<UserResponse>(It.IsAny<User>())).Returns(new UserResponse());
    }

    [Fact]
    public async Task LoginAsync_WhenEmailIsUnknown_ReturnsUnauthorized()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateSut().LoginAsync(new LoginRequest { Email = "nobody@x.com", Password = "whatever" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ReturnsUnauthorized()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "a@x.com", PasswordHash = "hash" });
        _hasher.Setup(h => h.Verify("bad", "hash")).Returns(false);

        var result = await CreateSut().LoginAsync(new LoginRequest { Email = "a@x.com", Password = "bad" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsAToken()
    {
        _users.Setup(r => r.GetByEmailAsync("a@x.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "a@x.com", PasswordHash = "hash", Role = UserRole.Customer });
        _hasher.Setup(h => h.Verify("good", "hash")).Returns(true);

        var result = await CreateSut().LoginAsync(new LoginRequest { Email = "a@x.com", Password = "good" });

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token", result.Value!.Token);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsTaken_ReturnsConflict()
    {
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().RegisterAsync(new RegisterRequest
        {
            Email = "taken@x.com", Password = "secret12", DisplayName = "T"
        });

        Assert.Equal(ResultStatus.Conflict, result.Status);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_HashesThePassword_CreatesAClient_AndSaves()
    {
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _hasher.Setup(h => h.Hash("secret12")).Returns("hashed");
        User? added = null;
        _users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => added = u)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().RegisterAsync(new RegisterRequest
        {
            Email = "New@X.com", Password = "secret12", DisplayName = "New User"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal("new@x.com", added!.Email);          // normalised to lower-case
        Assert.Equal("hashed", added.PasswordHash);        // stored the hash, not the plain text
        Assert.Equal(UserRole.Customer, added.Role);         // self-registration is always a Client
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

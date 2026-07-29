using Moq;
using NtbEvent.Application.Auth.Dtos;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Services;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Tests.Unit.Auth;

/// <summary>Mid-term report §4.2.1 UT-01..UT-03 (Authentication).</summary>
public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _passwordService.Object,
            _tokenService.Object);
    }

    private static User ActiveUser() => new()
    {
        Id = 1,
        Email = "admin@ntb.gov.np",
        NormalizedEmail = "ADMIN@NTB.GOV.NP",
        PasswordHash = "hashed",
        FullName = "Admin User",
        Role = UserRole.Admin,
        IsActive = true
    };

    // UT-01: Login with valid credentials -> JWT token and refresh token returned.
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        var user = ActiveUser();
        _userRepository.Setup(r => r.GetByNormalizedEmailAsync("ADMIN@NTB.GOV.NP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordService.Setup(p => p.VerifyPassword(user, user.PasswordHash, "correct-password")).Returns(true);
        _tokenService.Setup(t => t.GenerateAccessToken(user))
            .Returns(new AccessTokenResult { Token = "access-token", ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15) });
        _tokenService.Setup(t => t.GenerateRefreshToken(null))
            .Returns(new RefreshTokenResult
            {
                Token = "refresh-token",
                TokenHash = "refresh-token-hash",
                TokenFamily = "family-1",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
            });

        var result = await _sut.LoginAsync(
            new LoginRequest { Email = "admin@ntb.gov.np", Password = "correct-password" },
            "127.0.0.1",
            "test-agent");

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        _refreshTokenRepository.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(t => t.TokenHash == "refresh-token-hash" && t.UserId == user.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // UT-02: Login with invalid password -> 401 Unauthorized (UnauthorizedAccessException).
    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorized()
    {
        var user = ActiveUser();
        _userRepository.Setup(r => r.GetByNormalizedEmailAsync("ADMIN@NTB.GOV.NP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordService.Setup(p => p.VerifyPassword(user, user.PasswordHash, "wrong-password")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync(
                new LoginRequest { Email = "admin@ntb.gov.np", Password = "wrong-password" },
                "127.0.0.1",
                "test-agent"));

        _tokenService.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    // UT-03: Refresh with expired token -> 401 Unauthorized, new token not issued.
    [Fact]
    public async Task RefreshAsync_ExpiredToken_ThrowsUnauthorizedAndIssuesNoNewToken()
    {
        var expiredToken = new RefreshToken
        {
            Id = 10,
            UserId = 1,
            TokenHash = "expired-hash",
            TokenFamily = "family-1",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
            RevokedAtUtc = null
        };
        _tokenService.Setup(t => t.ComputeRefreshTokenHash("expired-token")).Returns("expired-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("expired-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);
        _refreshTokenRepository.Setup(r => r.GetByFamilyAsync("family-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([expiredToken]);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.RefreshAsync(
                new RefreshSessionRequest { RefreshToken = "expired-token" },
                "127.0.0.1",
                "test-agent"));

        _tokenService.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _tokenService.Verify(t => t.GenerateRefreshToken(It.IsAny<string?>()), Times.Never);
    }
}

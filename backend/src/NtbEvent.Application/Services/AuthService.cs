using NtbEvent.Application.Auth.Dtos;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Users;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthSessionDto> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordService.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        EnsureUserIsActive(user);

        user.LastLoginAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;

        return await CreateSessionAsync(user, ipAddress, userAgent, null, cancellationToken);
    }

    public async Task<AuthSessionDto> RefreshAsync(
        RefreshSessionRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is required.");
        }

        var tokenHash = _tokenService.ComputeRefreshTokenHash(request.RefreshToken.Trim());
        var currentToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (currentToken is null)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        if (currentToken.RevokedAtUtc.HasValue || currentToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await RevokeFamilyAsync(
                currentToken.TokenFamily,
                "Refresh token reuse detected.",
                ipAddress,
                cancellationToken);

            throw new UnauthorizedAccessException("Refresh token is no longer valid.");
        }

        var user = currentToken.User ?? await _userRepository.GetByIdAsync(currentToken.UserId, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("User account not found.");
        }

        EnsureUserIsActive(user);

        currentToken.RevokedAtUtc = DateTime.UtcNow;
        currentToken.RevokedByIp = ipAddress;
        currentToken.ReplacedByTokenHash = string.Empty;
        currentToken.RevocationReason = "Rotated by refresh.";
        currentToken.UpdatedAtUtc = DateTime.UtcNow;

        return await CreateSessionAsync(user, ipAddress, userAgent, currentToken, cancellationToken);
    }

    public async Task LogoutAsync(
        LogoutRequest request,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var tokenHash = _tokenService.ComputeRefreshTokenHash(request.RefreshToken.Trim());
        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (refreshToken is null || refreshToken.RevokedAtUtc.HasValue)
        {
            return;
        }

        refreshToken.RevokedAtUtc = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.RevocationReason = "Logged out.";
        refreshToken.UpdatedAtUtc = DateTime.UtcNow;

        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        return UserMappings.ToAuthUserDto(user);
    }

    private async Task<AuthSessionDto> CreateSessionAsync(
        User user,
        string ipAddress,
        string userAgent,
        RefreshToken? rotatingToken,
        CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(rotatingToken?.TokenFamily);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshToken.TokenHash,
            TokenFamily = refreshToken.TokenFamily,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        if (rotatingToken is not null)
        {
            rotatingToken.ReplacedByTokenHash = refreshToken.TokenHash;
        }

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new AuthSessionDto
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            User = UserMappings.ToAuthUserDto(user)
        };
    }

    private async Task RevokeFamilyAsync(
        string tokenFamily,
        string reason,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var familyTokens = await _refreshTokenRepository.GetByFamilyAsync(tokenFamily, cancellationToken);

        foreach (var familyToken in familyTokens.Where(token => !token.RevokedAtUtc.HasValue))
        {
            familyToken.RevokedAtUtc = DateTime.UtcNow;
            familyToken.RevokedByIp = ipAddress;
            familyToken.RevocationReason = reason;
            familyToken.UpdatedAtUtc = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(familyToken, cancellationToken);
        }
    }

    private static void EnsureUserIsActive(User user)
    {
        if (!user.IsActive)
        {
            throw new InvalidOperationException("This user account is inactive.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}

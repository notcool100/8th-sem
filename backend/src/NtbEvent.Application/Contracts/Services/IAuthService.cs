using NtbEvent.Application.Auth.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IAuthService
{
    Task<AuthSessionDto> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task<AuthSessionDto> RefreshAsync(
        RefreshSessionRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        LogoutRequest request,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthUserDto?> GetCurrentUserAsync(long userId, CancellationToken cancellationToken = default);
}

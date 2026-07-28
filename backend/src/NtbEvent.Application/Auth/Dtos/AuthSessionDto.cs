namespace NtbEvent.Application.Auth.Dtos;

public sealed class AuthSessionDto
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public DateTime RefreshTokenExpiresAtUtc { get; init; }

    public AuthUserDto User { get; init; } = new();
}

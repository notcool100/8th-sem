namespace NtbEvent.Application.Auth.Dtos;

public sealed class RefreshTokenResult
{
    public string Token { get; init; } = string.Empty;

    public string TokenHash { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public string TokenFamily { get; init; } = string.Empty;
}

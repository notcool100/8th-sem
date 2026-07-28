namespace NtbEvent.Application.Auth.Dtos;

public sealed class AccessTokenResult
{
    public string Token { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }
}

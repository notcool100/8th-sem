namespace NtbEvent.Application.Auth.Dtos;

public sealed class LogoutRequest
{
    public string? RefreshToken { get; init; }
}

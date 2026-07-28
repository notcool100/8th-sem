namespace NtbEvent.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "NtbEvent.Api";

    public string Audience { get; init; } = "NtbEvent.Frontend";

    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    public int RefreshTokenLifetimeDays { get; init; } = 30;
}

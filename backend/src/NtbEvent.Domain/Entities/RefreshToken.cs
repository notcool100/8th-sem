using NtbEvent.Domain.Common;

namespace NtbEvent.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public long UserId { get; set; }

    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public string TokenFamily { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public string CreatedByIp { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public DateTime? RevokedAtUtc { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? RevocationReason { get; set; }
}

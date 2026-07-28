using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Client;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAtUtc { get; set; }

    public long? CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }

    public ICollection<User> ManagedUsers { get; set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public ICollection<UserPermission> Permissions { get; set; } = [];
}

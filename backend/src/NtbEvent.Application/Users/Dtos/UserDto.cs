namespace NtbEvent.Application.Users.Dtos;

public sealed class UserDto
{
    public long Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime? LastLoginAtUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public List<UserPermissionDto> Permissions { get; init; } = [];
}

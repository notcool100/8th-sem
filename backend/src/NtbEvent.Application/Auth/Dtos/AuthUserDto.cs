using NtbEvent.Application.Users.Dtos;

namespace NtbEvent.Application.Auth.Dtos;

public sealed class AuthUserDto
{
    public long Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public List<UserPermissionDto> Permissions { get; init; } = [];
}

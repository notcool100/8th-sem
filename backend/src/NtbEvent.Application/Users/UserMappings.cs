using NtbEvent.Application.Auth.Dtos;
using NtbEvent.Application.Users.Dtos;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Users;

internal static class UserMappings
{
    public static AuthUserDto ToAuthUserDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Department = user.Department,
        Role = user.Role.ToString().ToLowerInvariant(),
        Permissions = user.Permissions
            .Select(p => new UserPermissionDto
            {
                NavItemKey = p.NavItem?.Key ?? string.Empty,
                NavItemLabel = p.NavItem?.Label ?? string.Empty,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanUpdate = p.CanUpdate,
                CanDelete = p.CanDelete,
                NeedsApproval = p.NeedsApproval
            })
            .ToList()
    };

    public static UserDto ToUserDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Department = user.Department,
        Role = user.Role.ToString().ToLowerInvariant(),
        IsActive = user.IsActive,
        LastLoginAtUtc = user.LastLoginAtUtc,
        CreatedAtUtc = user.CreatedAtUtc,
        Permissions = user.Permissions
            .Select(p => new UserPermissionDto
            {
                NavItemKey = p.NavItem?.Key ?? string.Empty,
                NavItemLabel = p.NavItem?.Label ?? string.Empty,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanUpdate = p.CanUpdate,
                CanDelete = p.CanDelete,
                NeedsApproval = p.NeedsApproval
            })
            .ToList()
    };
}

namespace NtbEvent.Application.Users.Dtos;

public sealed class UserPermissionDto
{
    public string NavItemKey { get; init; } = string.Empty;
    public string NavItemLabel { get; init; } = string.Empty;
    public bool CanView { get; init; }
    public bool CanCreate { get; init; }
    public bool CanUpdate { get; init; }
    public bool CanDelete { get; init; }
    public bool NeedsApproval { get; init; }
}

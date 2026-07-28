namespace NtbEvent.Application.Users.Dtos;

public sealed class SetPermissionRequest
{
    public int NavItemId { get; init; }
    public bool CanView { get; init; }
    public bool CanCreate { get; init; }
    public bool CanUpdate { get; init; }
    public bool CanDelete { get; init; }
    public bool NeedsApproval { get; init; }
}

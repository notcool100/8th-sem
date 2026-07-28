namespace NtbEvent.Application.Users.Dtos;

public sealed class SetUserPermissionsRequest
{
    public List<SetPermissionRequest> Permissions { get; init; } = [];
}

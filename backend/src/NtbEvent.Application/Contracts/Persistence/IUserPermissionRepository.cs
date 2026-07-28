using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IUserPermissionRepository
{
    Task<IReadOnlyList<UserPermission>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task ReplaceUserPermissionsAsync(long userId, IReadOnlyList<UserPermission> permissions, CancellationToken cancellationToken = default);
}

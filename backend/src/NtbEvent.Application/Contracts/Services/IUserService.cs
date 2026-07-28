using NtbEvent.Application.Users.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NavItemDto>> GetNavItemsAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, long createdByUserId, CancellationToken cancellationToken = default);
    Task SetUserPermissionsAsync(long userId, SetUserPermissionsRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateUserAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(long id, long requestingUserId, CancellationToken cancellationToken = default);
}

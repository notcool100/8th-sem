using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Users;
using NtbEvent.Application.Users.Dtos;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly INavItemRepository _navItemRepository;
    private readonly IUserPermissionRepository _userPermissionRepository;
    private readonly IPasswordService _passwordService;

    public UserService(
        IUserRepository userRepository,
        INavItemRepository navItemRepository,
        IUserPermissionRepository userPermissionRepository,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _navItemRepository = navItemRepository;
        _userPermissionRepository = userPermissionRepository;
        _passwordService = passwordService;
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(UserMappings.ToUserDto).ToList();
    }

    public async Task<IReadOnlyList<NavItemDto>> GetNavItemsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _navItemRepository.GetAllAsync(cancellationToken);
        return items.Select(n => new NavItemDto
        {
            Id = n.Id,
            Key = n.Key,
            Label = n.Label,
            Section = n.Section,
            SortOrder = n.SortOrder
        }).ToList();
    }

    public async Task<UserDto> CreateUserAsync(
        CreateUserRequest request,
        long createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        ValidatePassword(request.Password);

        var now = DateTime.UtcNow;
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Department = request.Department?.Trim() ?? string.Empty,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        if (request.Permissions.Count > 0)
        {
            var permissions = request.Permissions.Select(p => new UserPermission
            {
                UserId = createdUser.Id,
                NavItemId = p.NavItemId,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanUpdate = p.CanUpdate,
                CanDelete = p.CanDelete,
                NeedsApproval = p.NeedsApproval,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }).ToList();

            await _userPermissionRepository.ReplaceUserPermissionsAsync(createdUser.Id, permissions, cancellationToken);
        }

        var userWithPermissions = await _userRepository.GetByIdAsync(createdUser.Id, cancellationToken);
        return UserMappings.ToUserDto(userWithPermissions!);
    }

    public async Task SetUserPermissionsAsync(long userId, SetUserPermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException($"User with id '{userId}' not found.");

        var now = DateTime.UtcNow;
        var permissions = request.Permissions.Select(p => new UserPermission
        {
            UserId = userId,
            NavItemId = p.NavItemId,
            CanView = p.CanView,
            CanCreate = p.CanCreate,
            CanUpdate = p.CanUpdate,
            CanDelete = p.CanDelete,
            NeedsApproval = p.NeedsApproval,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        }).ToList();

        await _userPermissionRepository.ReplaceUserPermissionsAsync(userId, permissions, cancellationToken);
    }

    public async Task<UserDto> UpdateUserAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"User with id '{id}' not found.");

        user.FullName = request.FullName.Trim();
        user.Department = request.Department?.Trim() ?? string.Empty;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        var updated = await _userRepository.GetByIdAsync(id, cancellationToken);
        return UserMappings.ToUserDto(updated!);
    }

    public async Task DeleteUserAsync(long id, long requestingUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"User with id '{id}' not found.");

        if (user.Role == UserRole.SuperAdmin)
            throw new InvalidOperationException("The superadmin account cannot be deleted.");

        if (user.Id == requestingUserId)
            throw new InvalidOperationException("You cannot delete your own account.");

        await _userRepository.DeleteAsync(user, cancellationToken);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.");
        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Password must include at least one uppercase letter.");
        if (!password.Any(char.IsLower))
            throw new ArgumentException("Password must include at least one lowercase letter.");
        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Password must include at least one number.");
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}

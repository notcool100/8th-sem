using Microsoft.AspNetCore.SignalR;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Notifications;
using NtbEvent.Domain.Entities;
using NtbEvent.Infrastructure.Hubs;

namespace NtbEvent.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IUserRepository _userRepo;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationService(
        INotificationRepository repo,
        IUserRepository userRepo,
        IHubContext<NotificationHub> hub)
    {
        _repo = repo;
        _userRepo = userRepo;
        _hub = hub;
    }

    public async Task NotifyUserAsync(long userId, string type, string title, string message, string? link = null, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repo.AddAsync(notification, ct);

        var dto = MapDto(notification);
        await _hub.Clients.User(userId.ToString()).SendAsync("notification", dto, ct);
    }

    public async Task NotifyAdminsAsync(string type, string title, string message, string? link = null, CancellationToken ct = default)
    {
        var adminIds = await _userRepo.GetAdminUserIdsAsync(ct);

        foreach (var adminId in adminIds)
        {
            var notification = new Notification
            {
                UserId = adminId,
                Type = type,
                Title = title,
                Message = message,
                Link = link,
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _repo.AddAsync(notification, ct);

            var dto = MapDto(notification);
            await _hub.Clients.User(adminId.ToString()).SendAsync("notification", dto, ct);
        }
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(long userId, CancellationToken ct = default)
    {
        var items = await _repo.GetByUserIdAsync(userId, 50, ct);
        return items.Select(MapDto).ToList();
    }

    public Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default)
        => _repo.GetUnreadCountAsync(userId, ct);

    public Task MarkReadAsync(long notificationId, long userId, CancellationToken ct = default)
        => _repo.MarkReadAsync(notificationId, userId, ct);

    public Task MarkAllReadAsync(long userId, CancellationToken ct = default)
        => _repo.MarkAllReadAsync(userId, ct);

    public Task DeleteAsync(long notificationId, long userId, CancellationToken ct = default)
        => _repo.DeleteAsync(notificationId, userId, ct);

    private static NotificationDto MapDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        Link = n.Link,
        IsRead = n.IsRead,
        CreatedAtUtc = n.CreatedAtUtc
    };
}

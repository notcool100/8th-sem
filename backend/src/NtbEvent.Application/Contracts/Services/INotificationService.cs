using NtbEvent.Application.Notifications;

namespace NtbEvent.Application.Contracts.Services;

public interface INotificationService
{
    Task NotifyUserAsync(long userId, string type, string title, string message, string? link = null, CancellationToken ct = default);
    Task NotifyAdminsAsync(string type, string title, string message, string? link = null, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(long userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default);
    Task MarkReadAsync(long notificationId, long userId, CancellationToken ct = default);
    Task MarkAllReadAsync(long userId, CancellationToken ct = default);
    Task DeleteAsync(long notificationId, long userId, CancellationToken ct = default);
}

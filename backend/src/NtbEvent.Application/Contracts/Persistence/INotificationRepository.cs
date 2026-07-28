using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(long userId, int limit = 50, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task MarkReadAsync(long notificationId, long userId, CancellationToken ct = default);
    Task MarkAllReadAsync(long userId, CancellationToken ct = default);
    Task DeleteAsync(long notificationId, long userId, CancellationToken ct = default);
}

using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NtbEventDbContext _dbContext;

    public NotificationRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(long userId, int limit = 50, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(ct);
    }

    public Task<int> GetUnreadCountAsync(long userId, CancellationToken ct = default)
    {
        return _dbContext.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task MarkReadAsync(long notificationId, long userId, CancellationToken ct = default)
    {
        await _dbContext.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task MarkAllReadAsync(long userId, CancellationToken ct = default)
    {
        await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task DeleteAsync(long notificationId, long userId, CancellationToken ct = default)
    {
        await _dbContext.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}

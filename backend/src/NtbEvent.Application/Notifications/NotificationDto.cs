namespace NtbEvent.Application.Notifications;

public sealed class NotificationDto
{
    public long Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Link { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

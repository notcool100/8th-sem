namespace NtbEvent.Domain.Entities;

public sealed class Notification
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

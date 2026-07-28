namespace NtbEvent.Domain.Entities;

public sealed class UserPermission
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public int NavItemId { get; set; }
    public NavItem NavItem { get; set; } = null!;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool NeedsApproval { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

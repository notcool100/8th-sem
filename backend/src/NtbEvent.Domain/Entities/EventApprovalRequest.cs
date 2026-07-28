using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

public sealed class EventApprovalRequest
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public Event Event { get; set; } = null!;
    public long RequestedByUserId { get; set; }
    public User RequestedBy { get; set; } = null!;
    public ApprovalAction Action { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public EventLifecycleStatus OriginalStatus { get; set; }
    public long? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}

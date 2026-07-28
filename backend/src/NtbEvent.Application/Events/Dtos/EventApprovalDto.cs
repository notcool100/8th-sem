namespace NtbEvent.Application.Events.Dtos;

public sealed class EventApprovalDto
{
    public long Id { get; init; }
    public long EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public string EventSlug { get; init; } = string.Empty;
    public string RequestedByName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime RequestedAtUtc { get; init; }
    public DateTime? ReviewedAtUtc { get; init; }
}

namespace NtbEvent.Application.Events.Dtos;

public sealed class RejectApprovalRequest
{
    public string RejectionReason { get; init; } = string.Empty;
}

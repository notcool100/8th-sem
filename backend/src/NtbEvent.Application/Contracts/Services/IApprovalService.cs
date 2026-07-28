using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IApprovalService
{
    Task<IReadOnlyList<EventApprovalDto>> GetAllAsync(CancellationToken ct = default);
    Task ApproveAsync(long approvalId, long reviewerUserId, CancellationToken ct = default);
    Task RejectAsync(long approvalId, long reviewerUserId, string reason, CancellationToken ct = default);
}

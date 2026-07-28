using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events.Dtos;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class ApprovalService : IApprovalService
{
    private readonly IEventApprovalRepository _approvalRepository;
    private readonly INotificationService _notificationService;

    public ApprovalService(IEventApprovalRepository approvalRepository, INotificationService notificationService)
    {
        _approvalRepository = approvalRepository;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<EventApprovalDto>> GetAllAsync(CancellationToken ct = default)
    {
        var requests = await _approvalRepository.GetAllAsync(ct);
        return requests.Select(MapDto).ToList();
    }

    public async Task ApproveAsync(long approvalId, long reviewerUserId, CancellationToken ct = default)
    {
        var request = await _approvalRepository.GetByIdAsync(approvalId, ct)
            ?? throw new InvalidOperationException($"Approval request {approvalId} not found.");

        if (request.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("This request has already been reviewed.");

        if (request.Event is null)
            throw new InvalidOperationException("The associated event no longer exists.");

        request.Event.Status = EventLifecycleStatus.Published;
        request.Event.UpdatedAtUtc = DateTime.UtcNow;

        request.Status = ApprovalStatus.Approved;
        request.ReviewedByUserId = reviewerUserId;
        request.ReviewedAtUtc = DateTime.UtcNow;

        await _approvalRepository.UpdateAsync(request, ct);

        await _notificationService.NotifyUserAsync(
            request.RequestedByUserId,
            "approved",
            "Event Approved",
            $"Your event \"{request.Event.Title}\" has been approved and is now live!",
            "/admin/events",
            ct);
    }

    public async Task RejectAsync(long approvalId, long reviewerUserId, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A rejection reason is required.");

        var request = await _approvalRepository.GetByIdAsync(approvalId, ct)
            ?? throw new InvalidOperationException($"Approval request {approvalId} not found.");

        if (request.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("This request has already been reviewed.");

        if (request.Event is null)
            throw new InvalidOperationException("The associated event no longer exists.");

        request.Event.Status = request.OriginalStatus;
        request.Event.UpdatedAtUtc = DateTime.UtcNow;

        request.Status = ApprovalStatus.Rejected;
        request.ReviewedByUserId = reviewerUserId;
        request.RejectionReason = reason.Trim();
        request.ReviewedAtUtc = DateTime.UtcNow;

        await _approvalRepository.UpdateAsync(request, ct);

        await _notificationService.NotifyUserAsync(
            request.RequestedByUserId,
            "rejected",
            "Event Rejected",
            $"Your event \"{request.Event.Title}\" was rejected. Reason: {reason.Trim()}",
            "/admin/events",
            ct);
    }

    private static EventApprovalDto MapDto(Domain.Entities.EventApprovalRequest r) => new()
    {
        Id = r.Id,
        EventId = r.EventId,
        EventTitle = r.Event?.Title ?? string.Empty,
        EventSlug = r.Event?.Slug ?? string.Empty,
        RequestedByName = r.RequestedBy?.FullName ?? string.Empty,
        Action = r.Action.ToString().ToLowerInvariant(),
        Status = r.Status.ToString().ToLowerInvariant(),
        RejectionReason = r.RejectionReason,
        ReviewedByName = r.ReviewedBy?.FullName,
        RequestedAtUtc = r.RequestedAtUtc,
        ReviewedAtUtc = r.ReviewedAtUtc
    };
}

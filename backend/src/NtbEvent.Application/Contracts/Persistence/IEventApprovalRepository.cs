using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IEventApprovalRepository
{
    Task<IReadOnlyList<EventApprovalRequest>> GetAllAsync(CancellationToken ct = default);
    Task<EventApprovalRequest?> GetByIdAsync(long id, CancellationToken ct = default);
    Task AddAsync(EventApprovalRequest request, CancellationToken ct = default);
    Task UpdateAsync(EventApprovalRequest request, CancellationToken ct = default);
}

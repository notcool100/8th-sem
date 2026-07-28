using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class EventApprovalRepository : IEventApprovalRepository
{
    private readonly NtbEventDbContext _dbContext;

    public EventApprovalRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventApprovalRequest>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.EventApprovalRequests
            .AsNoTracking()
            .Include(r => r.Event)
            .Include(r => r.RequestedBy)
            .Include(r => r.ReviewedBy)
            .OrderByDescending(r => r.RequestedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<EventApprovalRequest?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _dbContext.EventApprovalRequests
            .Include(r => r.Event)
            .Include(r => r.RequestedBy)
            .Include(r => r.ReviewedBy)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AddAsync(EventApprovalRequest request, CancellationToken ct = default)
    {
        _dbContext.EventApprovalRequests.Add(request);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(EventApprovalRequest request, CancellationToken ct = default)
    {
        _dbContext.EventApprovalRequests.Update(request);
        await _dbContext.SaveChangesAsync(ct);
    }
}

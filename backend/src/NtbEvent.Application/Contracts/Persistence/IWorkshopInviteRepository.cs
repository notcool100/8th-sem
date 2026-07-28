using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IWorkshopInviteRepository
{
    /// <summary>All recipients imported for an event, newest first.</summary>
    Task<IReadOnlyList<WorkshopInvite>> GetByEventAsync(long eventId, CancellationToken cancellationToken = default);

    /// <summary>All recipients across every event, newest first (Event included).</summary>
    Task<IReadOnlyList<WorkshopInvite>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<WorkshopInvite?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Loads a recipient by its secret QR/invite token, with Event included.</summary>
    Task<WorkshopInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Normalized emails already imported for this event (used to skip duplicates on import).</summary>
    Task<HashSet<string>> GetNormalizedEmailsByEventAsync(long eventId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IReadOnlyList<WorkshopInvite> invites, CancellationToken cancellationToken = default);

    Task UpdateAsync(WorkshopInvite invite, CancellationToken cancellationToken = default);
}

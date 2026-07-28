using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface INavItemRepository
{
    Task<IReadOnlyList<NavItem>> GetAllAsync(CancellationToken cancellationToken = default);
}

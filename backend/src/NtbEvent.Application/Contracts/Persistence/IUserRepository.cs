using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task DeleteAsync(User user, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<long>> GetAdminUserIdsAsync(CancellationToken cancellationToken = default);
}

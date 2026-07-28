using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetByFamilyAsync(string tokenFamily, CancellationToken cancellationToken = default);

    Task<RefreshToken> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly NtbEventDbContext _dbContext;

    public RefreshTokenRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByFamilyAsync(
        string tokenFamily,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(token => token.TokenFamily == tokenFamily)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken> AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return refreshToken;
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class FestivalRepository : IFestivalRepository
{
    private readonly NtbEventDbContext _dbContext;

    public FestivalRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Festival>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Festivals
            .OrderBy(f => f.DateAd)
            .ThenBy(f => f.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Festival?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Festivals
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Festivals
            .FirstOrDefaultAsync(f => f.Slug == slug, cancellationToken);
    }

    public async Task<Festival> AddAsync(Festival entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Festivals.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> UpdateAsync(Festival entity, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Festivals
            .FirstOrDefaultAsync(f => f.Id == entity.Id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        _dbContext.Entry(existing).CurrentValues.SetValues(entity);
        existing.Image = entity.Image;
        existing.Highlights = entity.Highlights;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Festivals
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        _dbContext.Festivals.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

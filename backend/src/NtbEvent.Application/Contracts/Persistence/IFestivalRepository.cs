using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IFestivalRepository
{
    Task<List<Festival>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Festival?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Festival> AddAsync(Festival entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Festival entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}

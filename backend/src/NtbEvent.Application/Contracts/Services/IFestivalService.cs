using NtbEvent.Application.Festivals.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IFestivalService
{
    Task<IReadOnlyList<FestivalDto>> GetFestivalsAsync(CancellationToken cancellationToken = default);

    Task<FestivalDto?> GetFestivalByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<FestivalDto> CreateFestivalAsync(SaveFestivalRequest request, long createdByUserId, CancellationToken cancellationToken = default);

    Task<FestivalDto?> UpdateFestivalAsync(long id, SaveFestivalRequest request, long updatedByUserId, CancellationToken cancellationToken = default);

    Task<bool> DeleteFestivalAsync(long id, CancellationToken cancellationToken = default);
}

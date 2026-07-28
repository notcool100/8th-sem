using NtbEvent.Application.Reports.Dtos;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IReportsRepository
{
    Task<ReportsSummaryDto> GetSummaryAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}

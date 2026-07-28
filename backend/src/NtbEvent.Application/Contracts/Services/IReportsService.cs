using NtbEvent.Application.Reports.Dtos;

namespace NtbEvent.Application.Contracts.Services;

public interface IReportsService
{
    Task<ReportsSummaryDto> GetSummaryAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}

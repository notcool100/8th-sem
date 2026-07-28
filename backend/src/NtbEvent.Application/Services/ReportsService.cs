using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Reports.Dtos;

namespace NtbEvent.Application.Services;

public sealed class ReportsService : IReportsService
{
    private readonly IReportsRepository _repo;

    public ReportsService(IReportsRepository repo) => _repo = repo;

    public Task<ReportsSummaryDto> GetSummaryAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
        => _repo.GetSummaryAsync(from, to, cancellationToken);
}

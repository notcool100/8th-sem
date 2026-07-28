using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Reports.Dtos;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
        => _reportsService = reportsService;

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<ReportsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ReportsSummaryDto>>> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var summary = await _reportsService.GetSummaryAsync(from, to, cancellationToken);
        return Ok(ApiResponse<ReportsSummaryDto>.Success(summary));
    }
}

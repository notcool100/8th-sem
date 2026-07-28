using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.WorkshopInvites.Dtos;

namespace NtbEvent.Api.Controllers;

/// <summary>
/// Bulk, plain-text workshop invitation emails (no QR/check-in) — recipients are
/// imported once (e.g. from a registration spreadsheet) and an admin triggers
/// each send individually from the list.
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Roles = "SuperAdmin,Admin")]
public sealed class WorkshopInvitesController : ControllerBase
{
    private readonly IWorkshopInviteService _workshopInviteService;
    private readonly IExcelExportService _excelExportService;

    public WorkshopInvitesController(IWorkshopInviteService workshopInviteService, IExcelExportService excelExportService)
    {
        _workshopInviteService = workshopInviteService;
        _excelExportService = excelExportService;
    }

    /// <summary>Imports recipients for an event (skips emails already imported for it).</summary>
    [HttpPost("events/{eventId:long}/workshop-invites/import")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<int>>> Import(
        long eventId,
        [FromBody] ImportWorkshopInviteesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<int>.Failure("User context is missing."));
        }

        try
        {
            var imported = await _workshopInviteService.ImportAsync(eventId, request, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return Ok(ApiResponse<int>.Success(imported, $"Imported {imported} recipient(s)."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<int>.Failure(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<int>.Failure(exception.Message));
        }
    }

    /// <summary>List all imported recipients for an event.</summary>
    [HttpGet("events/{eventId:long}/workshop-invites")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopInviteDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopInviteDto>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopInviteDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkshopInviteDto>>>> ListForEvent(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<IReadOnlyList<WorkshopInviteDto>>.Failure("User context is missing."));
        }

        try
        {
            var invites = await _workshopInviteService.GetForEventAsync(eventId, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<WorkshopInviteDto>>.Success(invites));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<IReadOnlyList<WorkshopInviteDto>>.Failure(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IReadOnlyList<WorkshopInviteDto>>.Failure(exception.Message));
        }
    }

    /// <summary>Download the workshop invite list for an event as an .xlsx workbook.</summary>
    [HttpGet("events/{eventId:long}/workshop-invites/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportForEvent(long eventId, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var invites = await _workshopInviteService.GetForEventAsync(eventId, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            var bytes = _excelExportService.BuildWorkshopInvitesWorkbook(invites);
            var eventTitle = invites.Count > 0 ? invites[0].EventTitle : $"event-{eventId}";
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{eventTitle.ToSafeFileNameSegment()}-workshop-invites.xlsx");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>List all workshop invites across every event — backs the admin Attendees page.</summary>
    [HttpGet("workshop-invites")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopInviteDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkshopInviteDto>>>> ListAll(
        CancellationToken cancellationToken = default)
    {
        var invites = await _workshopInviteService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WorkshopInviteDto>>.Success(invites));
    }

    /// <summary>Sends (or re-sends) the workshop invitation email to one recipient.</summary>
    [HttpPost("workshop-invites/{id:long}/send")]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WorkshopInviteDto>>> Send(
        long id,
        [FromBody] SendWorkshopInviteRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<WorkshopInviteDto>.Failure("User context is missing."));
        }

        try
        {
            var invite = await _workshopInviteService.SendAsync(id, request, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return invite is null
                ? NotFound(ApiResponse<WorkshopInviteDto>.Failure("Recipient not found."))
                : Ok(ApiResponse<WorkshopInviteDto>.Success(invite, "Invitation sent."));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<WorkshopInviteDto>.Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<WorkshopInviteDto>.Failure(exception.Message));
        }
    }

    /// <summary>Returns the QR PNG for a recipient.</summary>
    [HttpGet("workshop-invites/{id:long}/qr")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQr(long id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var png = await _workshopInviteService.GetQrPngAsync(id, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return png is null ? NotFound() : File(png, "image/png");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Public: load a recipient by its QR/invite token (for the guest landing page).</summary>
    [AllowAnonymous]
    [HttpGet("workshop-invites/by-token/{token}")]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WorkshopInviteDto>>> GetByToken(
        string token,
        CancellationToken cancellationToken = default)
    {
        var invite = await _workshopInviteService.GetByTokenAsync(token, cancellationToken);
        return invite is null
            ? NotFound(ApiResponse<WorkshopInviteDto>.Failure("Workshop invite not found."))
            : Ok(ApiResponse<WorkshopInviteDto>.Success(invite));
    }

    /// <summary>
    /// Scan a QR (or paste the code) at the door. Returns the guest info for the
    /// confirmation popup but does NOT consume the QR.
    /// </summary>
    [HttpPost("workshop-invites/scan")]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteScanResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteScanResultDto>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<WorkshopInviteScanResultDto>>> Scan(
        [FromBody] ScanWorkshopInviteRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<WorkshopInviteScanResultDto>.Failure("User context is missing."));
        }

        try
        {
            var result = await _workshopInviteService.ScanAsync(request.Token, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return Ok(ApiResponse<WorkshopInviteScanResultDto>.Success(result));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<WorkshopInviteScanResultDto>.Failure(exception.Message));
        }
    }

    /// <summary>
    /// Confirm check-in: marks the recipient verified and expires the QR so it
    /// cannot be scanned again.
    /// </summary>
    [HttpPost("workshop-invites/{id:long}/verify")]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteScanResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteScanResultDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<WorkshopInviteScanResultDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WorkshopInviteScanResultDto>>> Verify(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<WorkshopInviteScanResultDto>.Failure("User context is missing."));
        }

        try
        {
            var result = await _workshopInviteService.VerifyAsync(id, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return result is null
                ? NotFound(ApiResponse<WorkshopInviteScanResultDto>.Failure("Recipient not found."))
                : Ok(ApiResponse<WorkshopInviteScanResultDto>.Success(result, result.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<WorkshopInviteScanResultDto>.Failure(exception.Message));
        }
    }
}

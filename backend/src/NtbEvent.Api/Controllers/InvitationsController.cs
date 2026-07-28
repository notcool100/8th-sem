using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Invitations.Dtos;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;
    private readonly IExcelExportService _excelExportService;

    public InvitationsController(IInvitationService invitationService, IExcelExportService excelExportService)
    {
        _invitationService = invitationService;
        _excelExportService = excelExportService;
    }

    /// <summary>Invite a guest to an event (creates token + QR and emails them).</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("events/{eventId:long}/invitations")]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InvitationDto>>> Invite(
        long eventId,
        [FromBody] InviteGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<InvitationDto>.Failure("User context is missing."));
        }

        try
        {
            var created = await _invitationService.InviteAsync(eventId, request, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<InvitationDto>.Success(created, "Invitation sent successfully."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<InvitationDto>.Failure(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<InvitationDto>.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<InvitationDto>.Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<InvitationDto>.Failure(exception.Message));
        }
    }

    /// <summary>List all invitations for an event.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("events/{eventId:long}/invitations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvitationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvitationDto>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvitationDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvitationDto>>>> ListForEvent(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<IReadOnlyList<InvitationDto>>.Failure("User context is missing."));
        }

        try
        {
            var invitations = await _invitationService.GetForEventAsync(eventId, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<InvitationDto>>.Success(invitations));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<IReadOnlyList<InvitationDto>>.Failure(exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<IReadOnlyList<InvitationDto>>.Failure(exception.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IReadOnlyList<InvitationDto>>.Failure($"Failed to load invitations: {ex.Message}"));
        }
    }

    /// <summary>Download the invitation list for an event as an .xlsx workbook.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("events/{eventId:long}/invitations/export")]
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
            var invitations = await _invitationService.GetForEventAsync(eventId, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            var bytes = _excelExportService.BuildInvitationsWorkbook(invitations);
            var eventTitle = invitations.Count > 0 ? invitations[0].EventTitle : $"event-{eventId}";
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{eventTitle.ToSafeFileNameSegment()}-invitations.xlsx");
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

    /// <summary>List all invitations across every event — backs the admin Attendees page.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("invitations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvitationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvitationDto>>>> ListAll(
        CancellationToken cancellationToken = default)
    {
        var invitations = await _invitationService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InvitationDto>>.Success(invitations));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("invitations/{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvitationDto>>> GetById(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<InvitationDto>.Failure("User context is missing."));
        }

        try
        {
            var invitation = await _invitationService.GetByIdAsync(id, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return invitation is null
                ? NotFound(ApiResponse<InvitationDto>.Failure("Invitation not found."))
                : Ok(ApiResponse<InvitationDto>.Success(invitation));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<InvitationDto>.Failure(exception.Message));
        }
    }

    /// <summary>Public: load an invitation by its token (for the guest landing page).</summary>
    [AllowAnonymous]
    [HttpGet("invitations/by-token/{token}")]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvitationDto>>> GetByToken(
        string token,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationService.GetByTokenAsync(token, cancellationToken);
        return invitation is null
            ? NotFound(ApiResponse<InvitationDto>.Failure("Invitation not found."))
            : Ok(ApiResponse<InvitationDto>.Success(invitation));
    }

    /// <summary>Returns the QR PNG for an invitation.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("invitations/{id:long}/qr")]
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
            var png = await _invitationService.GetQrPngAsync(id, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return png is null ? NotFound() : File(png, "image/png");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Re-send the invitation email (link + QR).</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("invitations/{id:long}/resend")]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InvitationDto>>> Resend(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<InvitationDto>.Failure("User context is missing."));
        }

        try
        {
            var invitation = await _invitationService.ResendAsync(id, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return invitation is null
                ? NotFound(ApiResponse<InvitationDto>.Failure("Invitation not found."))
                : Ok(ApiResponse<InvitationDto>.Success(invitation, "Invitation re-sent."));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<InvitationDto>.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<InvitationDto>.Failure(exception.Message));
        }
    }

    /// <summary>
    /// Scan a QR (or paste the code) at the door. Returns the guest info for the
    /// confirmation popup but does NOT consume the QR.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("invitations/scan")]
    [ProducesResponseType(typeof(ApiResponse<ScanResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ScanResultDto>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ScanResultDto>>> Scan(
        [FromBody] ScanRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<ScanResultDto>.Failure("User context is missing."));
        }

        try
        {
            var result = await _invitationService.ScanAsync(request.Token, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return Ok(ApiResponse<ScanResultDto>.Success(result));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<ScanResultDto>.Failure(exception.Message));
        }
    }

    /// <summary>
    /// Confirm check-in: marks the invitation verified and expires the QR so it
    /// cannot be scanned again.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("invitations/{id:long}/verify")]
    [ProducesResponseType(typeof(ApiResponse<ScanResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ScanResultDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScanResultDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ScanResultDto>>> Verify(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<ScanResultDto>.Failure("User context is missing."));
        }

        try
        {
            var result = await _invitationService.VerifyAsync(id, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return result is null
                ? NotFound(ApiResponse<ScanResultDto>.Failure("Invitation not found."))
                : Ok(ApiResponse<ScanResultDto>.Success(result, result.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<ScanResultDto>.Failure(exception.Message));
        }
    }

    /// <summary>Revoke an invitation.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpDelete("invitations/{id:long}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Cancel(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse.Failure("User context is missing."));
        }

        try
        {
            var cancelled = await _invitationService.CancelAsync(id, userId.Value, User.IsInRole("SuperAdmin"), cancellationToken);
            return cancelled
                ? Ok(ApiResponse.Success("Invitation cancelled."))
                : NotFound(ApiResponse.Failure("Invitation not found."));
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Failure(exception.Message));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Registrations.Dtos;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class RegistrationsController : ControllerBase
{
    private readonly IEventRegistrationService _registrationService;
    private readonly IExcelExportService _excelExportService;

    public RegistrationsController(IEventRegistrationService registrationService, IExcelExportService excelExportService)
    {
        _registrationService = registrationService;
        _excelExportService = excelExportService;
    }

    /// <summary>Public: self-register for an event that has registration enabled.</summary>
    [AllowAnonymous]
    [HttpPost("events/{eventId:long}/registrations")]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<EventRegistrationDto>>> Register(
        long eventId,
        [FromBody] RegisterGuestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _registrationService.RegisterAsync(eventId, request, cancellationToken);
            return CreatedAtAction(
                nameof(Register),
                new { eventId },
                ApiResponse<EventRegistrationDto>.Success(created, "Registration submitted. Awaiting admin approval."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<EventRegistrationDto>.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<EventRegistrationDto>.Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<EventRegistrationDto>.Failure(exception.Message));
        }
    }

    /// <summary>List all registrations for an event.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("events/{eventId:long}/registrations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EventRegistrationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventRegistrationDto>>>> ListForEvent(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        var registrations = await _registrationService.GetForEventAsync(eventId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventRegistrationDto>>.Success(registrations));
    }

    /// <summary>Download the registration list for an event as an .xlsx workbook.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("events/{eventId:long}/registrations/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportForEvent(long eventId, CancellationToken cancellationToken = default)
    {
        var registrations = await _registrationService.GetForEventAsync(eventId, cancellationToken);
        var bytes = _excelExportService.BuildRegistrationsWorkbook(registrations);
        var eventTitle = registrations.Count > 0 ? registrations[0].EventTitle : $"event-{eventId}";
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{eventTitle.ToSafeFileNameSegment()}-registrations.xlsx");
    }

    /// <summary>List all registrations across every event — backs the admin Attendees page.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("registrations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EventRegistrationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventRegistrationDto>>>> ListAll(
        CancellationToken cancellationToken = default)
    {
        var registrations = await _registrationService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventRegistrationDto>>.Success(registrations));
    }

    /// <summary>Approve a pending registration.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("registrations/{id:long}/approve")]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EventRegistrationDto>>> Approve(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<EventRegistrationDto>.Failure("User context is missing."));
        }

        var registration = await _registrationService.ApproveAsync(id, userId.Value, cancellationToken);
        return registration is null
            ? NotFound(ApiResponse<EventRegistrationDto>.Failure("Registration not found."))
            : Ok(ApiResponse<EventRegistrationDto>.Success(registration, "Registration approved."));
    }

    /// <summary>Reject a pending registration.</summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("registrations/{id:long}/reject")]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EventRegistrationDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EventRegistrationDto>>> Reject(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<EventRegistrationDto>.Failure("User context is missing."));
        }

        var registration = await _registrationService.RejectAsync(id, userId.Value, cancellationToken);
        return registration is null
            ? NotFound(ApiResponse<EventRegistrationDto>.Failure("Registration not found."))
            : Ok(ApiResponse<EventRegistrationDto>.Success(registration, "Registration rejected."));
    }
}

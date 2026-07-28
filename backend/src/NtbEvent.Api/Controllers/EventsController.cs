using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events;
using NtbEvent.Application.Events.Dtos;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IEventApprovalRepository _approvalRepository;
    private readonly IUserPermissionRepository _permissionRepository;
    private readonly INotificationService _notificationService;

    public EventsController(
        IEventService eventService,
        IEventApprovalRepository approvalRepository,
        IUserPermissionRepository permissionRepository,
        INotificationService notificationService)
    {
        _eventService = eventService;
        _approvalRepository = approvalRepository;
        _permissionRepository = permissionRepository;
        _notificationService = notificationService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventDto>>>> GetEvents(
        [FromQuery] string? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] bool includeDrafts = true,
        [FromQuery] string? region = null,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var events = await _eventService.GetEventsAsync(
            BuildFilter(category, fromDate, includeDrafts, region, search, status, toDate),
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<EventDto>>.Success(events));
    }

    [AllowAnonymous]
    [HttpGet("public")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventDto>>>> GetPublicEvents(
        [FromQuery] string? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] string? region = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(category, fromDate, false, region, search, "published", toDate);
        var events = await _eventService.GetEventsAsync(filter, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventDto>>.Success(events));
    }

    [AllowAnonymous]
    [HttpGet("public/list")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<EventDto>>>> GetPublicEventsList(
        [FromQuery] string? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? region,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "date",
        [FromQuery] string sortDir = "asc",
        CancellationToken cancellationToken = default)
    {
        var filter = new EventFilter
        {
            Category = category,
            FromDate = fromDate,
            ToDate = toDate,
            Region = region,
            Search = search,
            IncludeDrafts = false,
            Status = "published",
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 100),
            SortBy = sortBy,
            SortDir = sortDir
        };

        var result = await _eventService.GetPagedEventsAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResult<EventDto>>.Success(result));
    }

    /// <summary>Public: load an event by its slug (for the shareable registration link).</summary>
    [AllowAnonymous]
    [HttpGet("public/by-slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EventDto>>> GetPublicEventBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var @event = await _eventService.GetEventBySlugAsync(slug, cancellationToken);
        return @event is null
            ? NotFound(ApiResponse<EventDto>.Failure("Event not found."))
            : Ok(ApiResponse<EventDto>.Success(@event));
    }

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EventDto>>> GetEventById(
        long id,
        CancellationToken cancellationToken = default)
    {
        var @event = await _eventService.GetEventByIdAsync(id, cancellationToken);
        return @event is null
            ? NotFound(ApiResponse<EventDto>.Failure("Event not found."))
            : Ok(ApiResponse<EventDto>.Success(@event));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<EventDto>>> CreateEvent(
        [FromBody] SaveEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var createdByUserId = User.GetUserId();
        if (!createdByUserId.HasValue)
            return Unauthorized(ApiResponse<EventDto>.Failure("User context is missing."));

        var wantsToPublish = string.Equals(request.Status, "published", StringComparison.OrdinalIgnoreCase);
        var needsApproval = wantsToPublish &&
                            !User.IsInRole("SuperAdmin") &&
                            await UserNeedsApprovalAsync(createdByUserId.Value, cancellationToken);

        if (needsApproval)
            request.Status = "pendingapproval";

        try
        {
            var created = await _eventService.CreateEventAsync(request, createdByUserId.Value, cancellationToken);

            if (needsApproval)
            {
                await _approvalRepository.AddAsync(new EventApprovalRequest
                {
                    EventId = created.Id,
                    RequestedByUserId = createdByUserId.Value,
                    Action = ApprovalAction.Create,
                    Status = ApprovalStatus.Pending,
                    OriginalStatus = EventLifecycleStatus.Draft,
                    RequestedAtUtc = DateTime.UtcNow
                }, cancellationToken);

                var submitterName = User.FindFirstValue(ClaimTypes.Name) ?? "A team member";
                await _notificationService.NotifyAdminsAsync(
                    "approval_request",
                    "Event Pending Approval",
                    $"\"{created.Title}\" was submitted for approval by {submitterName}.",
                    "/admin/events/pending-approval",
                    cancellationToken);
            }

            return CreatedAtAction(nameof(GetEventById), new { id = created.Id },
                ApiResponse<EventDto>.Success(created,
                    needsApproval ? "Event submitted for approval." : "Event created successfully.",
                    needsApproval));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<EventDto>.Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<EventDto>.Failure(exception.Message));
        }
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<EventDto>>> UpdateEvent(
        long id,
        [FromBody] SaveEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var updatedByUserId = User.GetUserId();
        if (!updatedByUserId.HasValue)
            return Unauthorized(ApiResponse<EventDto>.Failure("User context is missing."));

        var wantsToPublish = string.Equals(request.Status, "published", StringComparison.OrdinalIgnoreCase);
        var needsApproval = wantsToPublish &&
                            !User.IsInRole("SuperAdmin") &&
                            await UserNeedsApprovalAsync(updatedByUserId.Value, cancellationToken);

        EventLifecycleStatus originalStatus = EventLifecycleStatus.Draft;
        if (needsApproval)
        {
            var existing = await _eventService.GetEventByIdAsync(id, cancellationToken);
            if (existing is not null && Enum.TryParse<EventLifecycleStatus>(existing.Status, true, out var parsed))
                originalStatus = parsed;
            request.Status = "pendingapproval";
        }

        try
        {
            var updated = await _eventService.UpdateEventAsync(id, request, updatedByUserId.Value, cancellationToken);
            if (updated is null)
                return NotFound(ApiResponse<EventDto>.Failure("Event not found."));

            if (needsApproval)
            {
                await _approvalRepository.AddAsync(new EventApprovalRequest
                {
                    EventId = updated.Id,
                    RequestedByUserId = updatedByUserId.Value,
                    Action = ApprovalAction.Update,
                    Status = ApprovalStatus.Pending,
                    OriginalStatus = originalStatus,
                    RequestedAtUtc = DateTime.UtcNow
                }, cancellationToken);

                var submitterName = User.FindFirstValue(ClaimTypes.Name) ?? "A team member";
                await _notificationService.NotifyAdminsAsync(
                    "approval_request",
                    "Event Update Pending Approval",
                    $"An update to \"{updated.Title}\" was submitted for approval by {submitterName}.",
                    "/admin/events/pending-approval",
                    cancellationToken);
            }

            return Ok(ApiResponse<EventDto>.Success(updated,
                needsApproval ? "Event update submitted for approval." : "Event updated successfully.",
                needsApproval));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<EventDto>.Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<EventDto>.Failure(exception.Message));
        }
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteEvent(
        long id,
        CancellationToken cancellationToken = default)
    {
        var deletedByUserId = User.GetUserId();
        if (!deletedByUserId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        try
        {
            var deleted = await _eventService.DeleteEventAsync(id, cancellationToken);
            return deleted
                ? Ok(ApiResponse.Success("Event deleted successfully."))
                : NotFound(ApiResponse.Failure("Event not found."));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ApiResponse { IsSuccess = false, Message = exception.Message });
        }
    }

    private async Task<bool> UserNeedsApprovalAsync(long userId, CancellationToken ct)
    {
        var perms = await _permissionRepository.GetByUserIdAsync(userId, ct);
        return perms.Any(p => p.NavItem?.Key == "events" && p.NeedsApproval);
    }

    private static EventFilter BuildFilter(
        string? category,
        DateTime? fromDate,
        bool includeDrafts,
        string? region,
        string? search,
        string? status,
        DateTime? toDate)
    {
        return new EventFilter
        {
            Category = category,
            FromDate = fromDate,
            IncludeDrafts = includeDrafts,
            Region = region,
            Search = search,
            Status = status,
            ToDate = toDate
        };
    }
}

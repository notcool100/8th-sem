using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Notifications;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<IReadOnlyList<NotificationDto>>.Failure("User context is missing."));

        var items = await _notificationService.GetForUserAsync(userId.Value, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Success(items));
    }

    [HttpPost("{id:long}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkRead(long id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        await _notificationService.MarkReadAsync(id, userId.Value, cancellationToken);
        return Ok(ApiResponse.Success("Notification marked as read."));
    }

    [HttpPost("read-all")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkAllRead(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        await _notificationService.MarkAllReadAsync(userId.Value, cancellationToken);
        return Ok(ApiResponse.Success("All notifications marked as read."));
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        await _notificationService.DeleteAsync(id, userId.Value, cancellationToken);
        return Ok(ApiResponse.Success("Notification deleted."));
    }
}

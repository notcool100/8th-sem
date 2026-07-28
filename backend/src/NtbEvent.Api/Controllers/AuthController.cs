using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Auth.Dtos;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AuthSessionDto>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _authService.LoginAsync(
                request,
                GetIpAddress(),
                GetUserAgent(),
                cancellationToken);

            return Ok(ApiResponse<AuthSessionDto>.Success(session, "Login successful."));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ApiResponse<AuthSessionDto>.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<AuthSessionDto>.Failure(exception.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthSessionDto>>> Refresh(
        [FromBody] RefreshSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _authService.RefreshAsync(
                request,
                GetIpAddress(),
                GetUserAgent(),
                cancellationToken);

            return Ok(ApiResponse<AuthSessionDto>.Success(session, "Session refreshed."));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(ApiResponse<AuthSessionDto>.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<AuthSessionDto>.Failure(exception.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        await _authService.LogoutAsync(request, GetIpAddress(), cancellationToken);
        return Ok(ApiResponse.Success("Logged out successfully."));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<AuthUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthUserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthUserDto>>> Me(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiResponse<AuthUserDto>.Failure("User context is missing."));
        }

        var user = await _authService.GetCurrentUserAsync(userId.Value, cancellationToken);
        return user is null
            ? Unauthorized(ApiResponse<AuthUserDto>.Failure("User account is unavailable."))
            : Ok(ApiResponse<AuthUserDto>.Success(user));
    }

    private string GetIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetUserAgent()
    {
        var userAgent = Request.Headers["X-Client-User-Agent"].FirstOrDefault()
            ?? Request.Headers.UserAgent.ToString();

        userAgent = userAgent.Trim();
        return userAgent.Length > 0
            ? userAgent[..Math.Min(userAgent.Length, 400)]
            : "unknown";
    }
}

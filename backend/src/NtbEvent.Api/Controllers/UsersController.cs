using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Users.Dtos;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetUsers(CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetUsersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(users));
    }

    [HttpGet("nav-items")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NavItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NavItemDto>>>> GetNavItems(CancellationToken cancellationToken = default)
    {
        var items = await _userService.GetNavItemsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NavItemDto>>.Success(items));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var createdByUserId = User.GetUserId();
        if (!createdByUserId.HasValue)
            return Unauthorized(ApiResponse<UserDto>.Failure("User context is missing."));

        try
        {
            var user = await _userService.CreateUserAsync(request, createdByUserId.Value, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<UserDto>.Success(user, "User created successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<UserDto>.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<UserDto>.Failure(exception.Message));
        }
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
        long id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
            return Ok(ApiResponse<UserDto>.Success(user, "User updated successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<UserDto>.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(ApiResponse<UserDto>.Failure(exception.Message));
        }
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(
        long id,
        CancellationToken cancellationToken = default)
    {
        var requestingUserId = User.GetUserId();
        if (!requestingUserId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        try
        {
            await _userService.DeleteUserAsync(id, requestingUserId.Value, cancellationToken);
            return Ok(ApiResponse.Success("User deleted successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse.Failure(exception.Message));
        }
    }

    [HttpPut("{id:long}/permissions")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> SetPermissions(
        long id,
        [FromBody] SetUserPermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _userService.SetUserPermissionsAsync(id, request, cancellationToken);
            return Ok(ApiResponse.Success("Permissions updated successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(ApiResponse.Failure(exception.Message));
        }
    }
}

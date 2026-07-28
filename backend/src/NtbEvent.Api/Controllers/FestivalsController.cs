using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Festivals.Dtos;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FestivalsController : ControllerBase
{
    private readonly IFestivalService _festivalService;

    public FestivalsController(IFestivalService festivalService)
    {
        _festivalService = festivalService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FestivalDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FestivalDto>>>> GetFestivals(
        CancellationToken cancellationToken = default)
    {
        var festivals = await _festivalService.GetFestivalsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FestivalDto>>.Success(festivals));
    }

    [AllowAnonymous]
    [HttpGet("public")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FestivalDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FestivalDto>>>> GetPublicFestivals(
        CancellationToken cancellationToken = default)
    {
        var festivals = await _festivalService.GetFestivalsAsync(cancellationToken);
        var published = festivals
            .Where(f => string.Equals(f.Status, "published", StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Ok(ApiResponse<IReadOnlyList<FestivalDto>>.Success(published));
    }

    [Authorize]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FestivalDto>>> GetFestivalById(
        long id,
        CancellationToken cancellationToken = default)
    {
        var festival = await _festivalService.GetFestivalByIdAsync(id, cancellationToken);
        return festival is null
            ? NotFound(ApiResponse<FestivalDto>.Failure("Festival not found."))
            : Ok(ApiResponse<FestivalDto>.Success(festival));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<FestivalDto>>> CreateFestival(
        [FromBody] SaveFestivalRequest request,
        CancellationToken cancellationToken = default)
    {
        var createdByUserId = User.GetUserId();
        if (!createdByUserId.HasValue)
            return Unauthorized(ApiResponse<FestivalDto>.Failure("User context is missing."));

        try
        {
            var created = await _festivalService.CreateFestivalAsync(request, createdByUserId.Value, cancellationToken);
            return CreatedAtAction(nameof(GetFestivalById), new { id = created.Id },
                ApiResponse<FestivalDto>.Success(created, "Festival created successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<FestivalDto>.Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<FestivalDto>.Failure(exception.Message));
        }
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<FestivalDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<FestivalDto>>> UpdateFestival(
        long id,
        [FromBody] SaveFestivalRequest request,
        CancellationToken cancellationToken = default)
    {
        var updatedByUserId = User.GetUserId();
        if (!updatedByUserId.HasValue)
            return Unauthorized(ApiResponse<FestivalDto>.Failure("User context is missing."));

        try
        {
            var updated = await _festivalService.UpdateFestivalAsync(id, request, updatedByUserId.Value, cancellationToken);
            return updated is null
                ? NotFound(ApiResponse<FestivalDto>.Failure("Festival not found."))
                : Ok(ApiResponse<FestivalDto>.Success(updated, "Festival updated successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<FestivalDto>.Failure(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<FestivalDto>.Failure(exception.Message));
        }
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteFestival(
        long id,
        CancellationToken cancellationToken = default)
    {
        var deletedByUserId = User.GetUserId();
        if (!deletedByUserId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        var deleted = await _festivalService.DeleteFestivalAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Success("Festival deleted successfully."))
            : NotFound(ApiResponse.Failure("Festival not found."));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Api.Extensions;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public sealed class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalsController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EventApprovalDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventApprovalDto>>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var approvals = await _approvalService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EventApprovalDto>>.Success(approvals));
    }

    [HttpPost("{id:long}/approve")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Approve(
        long id,
        CancellationToken cancellationToken = default)
    {
        var reviewerUserId = User.GetUserId();
        if (!reviewerUserId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        try
        {
            await _approvalService.ApproveAsync(id, reviewerUserId.Value, cancellationToken);
            return Ok(ApiResponse.Success("Event approved successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse.Failure(exception.Message));
        }
    }

    [HttpPost("{id:long}/reject")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Reject(
        long id,
        [FromBody] RejectApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        var reviewerUserId = User.GetUserId();
        if (!reviewerUserId.HasValue)
            return Unauthorized(ApiResponse.Failure("User context is missing."));

        try
        {
            await _approvalService.RejectAsync(id, reviewerUserId.Value, request.RejectionReason, cancellationToken);
            return Ok(ApiResponse.Success("Event rejected."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse.Failure(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse.Failure(exception.Message));
        }
    }
}

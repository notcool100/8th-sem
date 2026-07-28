using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.EmailTemplates.Dtos;

namespace NtbEvent.Api.Controllers;

/// <summary>
/// Admin-editable invitation email content (Workshop Invite / Event Invitation),
/// authored via a WYSIWYG editor in the admin UI.
/// </summary>
[ApiController]
[Route("api/email-templates")]
[Authorize(Roles = "SuperAdmin,Admin")]
public sealed class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailTemplatesController(IEmailTemplateService emailTemplateService)
    {
        _emailTemplateService = emailTemplateService;
    }

    [HttpGet("{type}")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> GetByType(
        string type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _emailTemplateService.GetByTypeAsync(type, cancellationToken);
            return Ok(ApiResponse<EmailTemplateDto>.Success(template));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<EmailTemplateDto>.Failure(exception.Message));
        }
    }

    [HttpPut("{type}")]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmailTemplateDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EmailTemplateDto>>> Update(
        string type,
        [FromBody] UpdateEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _emailTemplateService.UpdateAsync(type, request, cancellationToken);
            return Ok(ApiResponse<EmailTemplateDto>.Success(template, "Email template saved."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<EmailTemplateDto>.Failure(exception.Message));
        }
    }
}

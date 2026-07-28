using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtbEvent.Application.CalendarSettings.Dtos;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/calendar-filter-settings")]
public sealed class CalendarFilterSettingsController : ControllerBase
{
    private readonly ICalendarFilterSettingsService _service;

    public CalendarFilterSettingsController(ICalendarFilterSettingsService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/calendar-filter-settings
    /// Public — returns the current filter visibility settings.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CalendarFilterSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CalendarFilterSettingsDto>>> Get()
    {
        var dto = await _service.GetAsync();
        return Ok(ApiResponse<CalendarFilterSettingsDto>.Success(dto));
    }

    /// <summary>
    /// PUT /api/calendar-filter-settings
    /// Admin only — updates which filter sections are visible on the public calendar.
    /// </summary>
    [HttpPut]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CalendarFilterSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CalendarFilterSettingsDto>>> Update(
        [FromBody] CalendarFilterSettingsDto dto)
    {
        var updated = await _service.UpdateAsync(dto);
        return Ok(ApiResponse<CalendarFilterSettingsDto>.Success(updated));
    }
}

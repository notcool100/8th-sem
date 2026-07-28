using Microsoft.AspNetCore.Mvc;
using NtbEvent.Application.Common;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<string>>> UploadFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.Failure("No file was uploaded."));
        }

        try
        {
            // Ensure uploads directory exists inside wwwroot
            var wwwRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(wwwRootPath))
            {
                wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadsPath = Path.Combine(wwwRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename to avoid duplicates
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var relativeUrl = $"/uploads/{uniqueFileName}";
            return Ok(ApiResponse<string>.Success(relativeUrl));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<string>.Failure($"An error occurred while uploading file: {ex.Message}")
            );
        }
    }
}

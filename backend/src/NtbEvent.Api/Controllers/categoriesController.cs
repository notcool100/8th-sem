using Microsoft.AspNetCore.Mvc;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Categorie.Dtos;
using Microsoft.AspNetCore.Authorization;
namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase

{
    private readonly ICategoriesService _categoriesService;
    public CategoriesController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    [HttpGet]
[ProducesResponseType(
    typeof(ApiResponse<IReadOnlyList<CategoriesDto>>),
    StatusCodes.Status200OK)]
public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoriesDto>>>> GetCategories([FromQuery] string? type = null)
{
    var categories = await _categoriesService.GetCategoriesAsync(type);

    return Ok(
        ApiResponse<IReadOnlyList<CategoriesDto>>.Success(categories)
    );
}
[HttpPost]
[Authorize]
[ProducesResponseType(
    typeof(ApiResponse<CategoriesDto>),
    StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CategoriesDto>>> CreateCategories(CategoriesDto categoriesDto)
    {
        var createdCategories = await _categoriesService.CreateCategoriesAsync(categoriesDto);
        return Ok(ApiResponse<CategoriesDto>.Success(createdCategories));
    }


[HttpDelete("{id}")]
[Authorize]
[ProducesResponseType(
    typeof(ApiResponse<CategoriesDto>),
    StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteCategories(long id)
    {
        var deleteMessage =await _categoriesService.DeleteCategoriesAsync(id);
        return Ok(ApiResponse<string>.Success(deleteMessage));
    }

[HttpPut("{id}")]
[Authorize]
[ProducesResponseType(
    typeof(ApiResponse<CategoriesDto>),
    StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CategoriesDto>>> UpdateCategories(long id, CategoriesDto categoriesDto)
    {
        var updatedCategory = await _categoriesService.UpdateCategoriesAsync(id, categoriesDto);
        return Ok(ApiResponse<CategoriesDto>.Success(updatedCategory));
    }
}


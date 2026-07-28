using Microsoft.AspNetCore.Mvc;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Persistence;

namespace NtbEvent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TagsController : ControllerBase
{
    private readonly ITagsRepository _tagsRepository;

    public TagsController(ITagsRepository tagsRepository)
    {
        _tagsRepository = tagsRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetTags()
    {
        var tags = await _tagsRepository.GetAllNamesAsync();
        return Ok(ApiResponse<List<string>>.Success(tags));
    }
}

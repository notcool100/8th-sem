using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Categorie.Dtos;
using NtbEvent.Application.Categorie;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class CategoriesService : ICategoriesService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    private static CategoriesDto Map(Categories entity)
    {
        return new CategoriesDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Color = entity.Color,
            Icon = entity.Icon,
            Tag = entity.Tag,
            IsHoliday = entity.IsHoliday,
            Type = entity.Type.ToString().ToLowerInvariant()
        };
    }

    private static CategoryType ParseType(string value)
    {
        return Enum.TryParse<CategoryType>(value, true, out var parsed)
            ? parsed
            : CategoryType.Event;
    }

    public async Task<List<CategoriesDto>> GetCategoriesAsync(string? type = null)
    {
        CategoryType? parsedType = string.IsNullOrWhiteSpace(type) ? null : ParseType(type);
        var categories = await _categoryRepository.GetAllAsync(parsedType);
        return categories.Select(Map).ToList();

    }
    public async Task<CategoriesDto> CreateCategoriesAsync(CategoriesDto categoriesDto)
    {
        var category = await _categoryRepository.CreateAsync(new Categories
        {
            Name = categoriesDto.Name,
            Description = categoriesDto.Description,
            Color = categoriesDto.Color,
            Icon = categoriesDto.Icon,
            Tag = categoriesDto.Tag,
            IsHoliday = categoriesDto.IsHoliday,
            Type = ParseType(categoriesDto.Type)
        });
        return Map(category);
    }
    // public async Task<CategoriesDto?> GetCategoriesByIdAsync(long id)
    // {
    //     var categories = await _categoryRepository.GetByIdAsync(id, cancellationToken);
    //     return categories is not null ? CategoryMappings.ToCategoriesDto(categories) : null;

    // }

    public async Task<string> DeleteCategoriesAsync(long id)
    {
        var category = await _categoryRepository.DeleteAsync(id);

        return category;
    }

    public async Task<CategoriesDto> UpdateCategoriesAsync(long id, CategoriesDto categoriesDto)
    {
        var category = await _categoryRepository.UpdateAsync(id, new Categories
        {
            Name = categoriesDto.Name,
            Description = categoriesDto.Description,
            Color = categoriesDto.Color,
            Icon = categoriesDto.Icon,
            Tag = categoriesDto.Tag,
            IsHoliday = categoriesDto.IsHoliday,
            Type = ParseType(categoriesDto.Type)
        });
        return Map(category);
    }
}
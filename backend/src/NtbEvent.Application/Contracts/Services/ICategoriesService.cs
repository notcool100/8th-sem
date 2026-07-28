using NtbEvent.Application.Categorie.Dtos;


namespace NtbEvent.Application.Contracts.Services;

public interface ICategoriesService
{
    Task<List<CategoriesDto>> GetCategoriesAsync(string? type = null);
    Task<CategoriesDto> CreateCategoriesAsync(CategoriesDto categoriesDto);
    // Task<CategoriesDto?> GetCategoriesByIdAsync(long id);
    Task<string> DeleteCategoriesAsync(long id);
    Task<CategoriesDto> UpdateCategoriesAsync(long id, CategoriesDto categoriesDto);
}

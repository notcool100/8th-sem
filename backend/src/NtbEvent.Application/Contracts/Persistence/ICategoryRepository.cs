using NtbEvent.Application.Events;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;
namespace NtbEvent.Application.Contracts.Persistence;

public interface ICategoryRepository
{
    Task<List<Categories>> GetAllAsync(CategoryType? type = null);
    Task<Categories> CreateAsync(Categories category);
    Task<string> DeleteAsync(long id);
    Task<Categories> UpdateAsync(long id, Categories category);
}

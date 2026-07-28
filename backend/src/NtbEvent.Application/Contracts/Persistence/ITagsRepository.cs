using NtbEvent.Domain.Entities;
public interface ITagsRepository
{
    Task<Tags> CreateAsync(Tags tags);
    Task<List<Tags>> GetAllAsync();
    Task<List<string>> GetAllNamesAsync();
    Task<Tags?> GetByNameAsync(string name);
    Task<string?> GetByIdAsync(long id);
}
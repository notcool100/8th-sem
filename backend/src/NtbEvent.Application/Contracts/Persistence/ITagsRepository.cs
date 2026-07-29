using NtbEvent.Domain.Entities;
public interface ITagsRepository
{
    Task<Tags> CreateAsync(Tags tags);
    Task<List<Tags>> GetAllAsync();
    Task<List<string>> GetAllNamesAsync();
    Task<Tags?> GetByNameAsync(string name);
    Task<string?> GetByIdAsync(long id);

    /// <summary>Batch lookup of tag names per event, keyed by event id, via the EventTags join.</summary>
    Task<Dictionary<long, List<string>>> GetTagNamesByEventIdsAsync(IEnumerable<long> eventIds);
}
using NtbEvent.Domain.Entities;

namespace NtbEvent.Tests.TestSupport;

/// <summary>In-memory stand-in for <see cref="ITagsRepository"/> used by system-level tests.</summary>
public sealed class FakeTagsRepository : ITagsRepository
{
    private readonly List<Tags> _tags = [];
    private long _nextId = 1;

    public FakeTagsRepository(IEnumerable<string>? seedTagNames = null)
    {
        foreach (var name in seedTagNames ?? [])
        {
            _tags.Add(new Tags { Id = _nextId++, Name = name });
        }
    }

    public Task<Tags> CreateAsync(Tags tags)
    {
        tags.Id = _nextId++;
        _tags.Add(tags);
        return Task.FromResult(tags);
    }

    public Task<List<Tags>> GetAllAsync() => Task.FromResult(_tags.ToList());

    public Task<List<string>> GetAllNamesAsync() => Task.FromResult(_tags.Select(t => t.Name).ToList());

    public Task<Tags?> GetByNameAsync(string name) =>
        Task.FromResult(_tags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task<string?> GetByIdAsync(long id) => Task.FromResult(_tags.FirstOrDefault(t => t.Id == id)?.Name);

    public Task<Dictionary<long, List<string>>> GetTagNamesByEventIdsAsync(IEnumerable<long> eventIds) =>
        Task.FromResult(new Dictionary<long, List<string>>());
}

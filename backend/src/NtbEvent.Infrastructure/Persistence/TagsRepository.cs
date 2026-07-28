using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Events;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Infrastructure.Persistence;
public sealed class TagsRepository : ITagsRepository
{
     private readonly NtbEventDbContext _dbContext;
    private readonly IDataRepo _dataRepo;
    public TagsRepository(NtbEventDbContext dbContext, IDataRepo dataRepo)
    {
        _dbContext = dbContext;
        _dataRepo = dataRepo;
    }
    public async Task<List<string>> GetAllNamesAsync()
    {
        return await _dbContext.Tags
            .Select(t => t.Name)
            .Distinct()
            .ToListAsync();
    }
    public async Task<Tags> CreateAsync(Tags tags)
    {
        var entry = await _dbContext.Tags.AddAsync(tags);
        await _dbContext.SaveChangesAsync();
        return entry.Entity;
    }
    public async Task<Tags?> GetByNameAsync(string name)
    {
        return await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name);
    }
   public async Task<string?> GetByIdAsync(long id)
{
    return await _dbContext.Tags
        .Where(t => t.Id == id)
        .Select(t => t.Name)
        .FirstOrDefaultAsync();
}
    public async Task<List<Tags>> GetAllAsync()
    {
        return await _dbContext.Tags.ToListAsync();
    }
}
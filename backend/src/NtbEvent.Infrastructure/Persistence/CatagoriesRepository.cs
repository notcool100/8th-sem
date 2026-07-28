using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Events;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class CategoriesRepository : ICategoryRepository
{
    private readonly ITagsRepository _tagsRepository;
    private readonly NtbEventDbContext _dbContext;
    private readonly IDataRepo _dataRepo;
    public CategoriesRepository(NtbEventDbContext dbContext, IDataRepo dataRepo, ITagsRepository tagsRepository)
    {
        _dbContext = dbContext;
        _dataRepo = dataRepo;
        _tagsRepository = tagsRepository;
    }
    public async Task<List<Categories>> GetAllAsync(CategoryType? type = null)
    {
        var query = _dbContext.Categories.AsQueryable();
        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }
        var categories = await query.ToListAsync();

        foreach (var category in categories)
        {
            var tags = await (
                from ct in _dbContext.CatagoriesTags
                join t in _dbContext.Tags
                    on ct.TagId equals t.Id
                where ct.CategoryId == category.Id
                select t.Name
            ).ToArrayAsync();

            category.Tag = tags;
        }

        return categories;
    }
    public async Task<Categories> CreateAsync(Categories category)
    {
        var entry = await _dbContext.Categories.AddAsync(category);
        await _dbContext.SaveChangesAsync();
        foreach (var tag in category.Tag ?? Array.Empty<string>())
        {
            var existingTag = await _tagsRepository.GetByNameAsync(tag);
            if (existingTag != null)
            {
                var categoryTag = new CatagoriesTags
                {
                    CategoryId = entry.Entity.Id,
                    TagId = existingTag.Id
                };
                await CreateCategoryTagsAsync(categoryTag);
            }
            else
            {
                var newTag = new Tags { Name = tag };
                await _tagsRepository.CreateAsync(newTag);
                var categoryTag = new CatagoriesTags
                {
                    CategoryId = entry.Entity.Id,
                    TagId = newTag.Id
                };
                await CreateCategoryTagsAsync(categoryTag);
            }
        }
        await _dbContext.SaveChangesAsync();
        return entry.Entity;
    }


    public async Task<string> DeleteAsync(long id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(r => r.Id == id);
        if (category == null)
        {
            return "Category not found.";
        }
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return "Category deleted successfully.";
    }

    public async Task<Categories> UpdateAsync(long id, Categories category)
    {
        var existingCategory = await _dbContext.Categories.FirstOrDefaultAsync(r => r.Id == id);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException("Category not found.");
        }

        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;
        existingCategory.Color = category.Color;
        existingCategory.Icon = category.Icon;
        existingCategory.IsHoliday = category.IsHoliday;
        existingCategory.Type = category.Type;

        // Manage tags
        var existingCategoryTags = await _dbContext.CatagoriesTags
            .Where(ct => ct.CategoryId == id)
            .ToListAsync();

        var newTags = category.Tag ?? Array.Empty<string>();
        var resolvedTags = new List<Tags>();

        foreach (var tagName in newTags)
        {
            var existingTag = await _tagsRepository.GetByNameAsync(tagName);
            if (existingTag != null)
            {
                resolvedTags.Add(existingTag);
            }
            else
            {
                var newTag = new Tags { Name = tagName };
                await _tagsRepository.CreateAsync(newTag);
                resolvedTags.Add(newTag);
            }
        }

        // Remove old tag associations
        var tagsToKeepIds = resolvedTags.Select(t => t.Id).ToList();
        var tagsToRemove = existingCategoryTags.Where(ct => !tagsToKeepIds.Contains(ct.TagId)).ToList();
        _dbContext.CatagoriesTags.RemoveRange(tagsToRemove);

        // Add new tag associations
        var existingTagIds = existingCategoryTags.Select(ct => ct.TagId).ToList();
        foreach (var resolvedTag in resolvedTags)
        {
            if (!existingTagIds.Contains(resolvedTag.Id))
            {
                var categoryTag = new CatagoriesTags
                {
                    CategoryId = id,
                    TagId = resolvedTag.Id
                };
                await _dbContext.CatagoriesTags.AddAsync(categoryTag);
            }
        }

        await _dbContext.SaveChangesAsync();
        
        category.Id = id;
        return category;
    }

    public async Task<Categories> GetByIdAsync(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return null;
        }

        var tags = await (
            from ct in _dbContext.CatagoriesTags
            join t in _dbContext.Tags
                on ct.TagId equals t.Id
            where ct.CategoryId == category.Id
            select t.Name
        ).ToArrayAsync();

        category.Tag = tags;

        return category;
    }
    public async Task<CatagoriesTags?> GetCategoryTagsByCategoryIdAsync(long categoryId)
    {
        return await _dbContext.CatagoriesTags
            .Include(ct => ct.Tag)
            .FirstOrDefaultAsync(ct => ct.CategoryId == categoryId);
    }
    public async Task<CatagoriesTags?> CreateCategoryTagsAsync(CatagoriesTags categoryTags)
    {
        var entry = await _dbContext.CatagoriesTags.AddAsync(categoryTags);
        await _dbContext.SaveChangesAsync();
        return entry.Entity;
    }
}
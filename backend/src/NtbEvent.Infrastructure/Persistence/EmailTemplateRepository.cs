using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly NtbEventDbContext _dbContext;

    public EmailTemplateRepository(NtbEventDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EmailTemplate?> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmailTemplates
            .FirstOrDefaultAsync(template => template.Type == type, cancellationToken);
    }

    public async Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        _dbContext.EmailTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(template).State == EntityState.Detached)
        {
            _dbContext.EmailTemplates.Attach(template);
            _dbContext.Entry(template).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByTypeAsync(string type, CancellationToken cancellationToken = default);

    Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default);

    Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
}

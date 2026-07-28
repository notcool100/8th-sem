using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;
using NtbEvent.Infrastructure.Configuration;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly NtbEventDbContext _dbContext;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly SeedOptions _seedOptions;

    public DatabaseInitializer(
        NtbEventDbContext dbContext,
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IOptions<SeedOptions> seedOptions)
    {
        _dbContext = dbContext;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _passwordService = passwordService;
        _seedOptions = seedOptions.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Apply all pending migrations
        await _dbContext.Database.MigrateAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
        await SeedEventsAsync(cancellationToken);
        await SeedNavItemsAsync(cancellationToken);
    }

    private async Task SeedEventsAsync(CancellationToken cancellationToken)
    {
        if (!_seedOptions.SeedDemoEvents)
        {
            return;
        }

        var existingCount = await _dbContext.Events
            .CountAsync(cancellationToken);

        if (existingCount > 0)
        {
            return;
        }

        foreach (var seededEvent in SeedData.Events)
        {
            await _eventRepository.AddAsync(seededEvent, cancellationToken);
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(_seedOptions.SuperAdminEmail);
        var existingUser = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            FullName = _seedOptions.SuperAdminFullName.Trim(),
            Email = _seedOptions.SuperAdminEmail.Trim(),
            NormalizedEmail = normalizedEmail,
            Department = _seedOptions.SuperAdminDepartment.Trim(),
            Role = UserRole.SuperAdmin,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        user.PasswordHash = _passwordService.HashPassword(user, _seedOptions.SuperAdminPassword);

        await _userRepository.AddAsync(user, cancellationToken);
    }

    private async Task SeedNavItemsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var navItems = new[]
        {
            new NavItem { Key = "dashboard",     Label = "Dashboard",                 Section = "Main",       SortOrder = 1,  CreatedAtUtc = now },
            new NavItem { Key = "events",        Label = "Events Management",          Section = "Content",    SortOrder = 2,  CreatedAtUtc = now },
            new NavItem { Key = "festivals",     Label = "Festivals Management",       Section = "Content",    SortOrder = 2,  CreatedAtUtc = now },
            new NavItem { Key = "calendar",      Label = "Calendar Management",        Section = "Content",    SortOrder = 3,  CreatedAtUtc = now },
            new NavItem { Key = "destinations",  Label = "Destinations",               Section = "Content",    SortOrder = 4,  CreatedAtUtc = now },
            new NavItem { Key = "email_templates", Label = "Email Templates",          Section = "Content",    SortOrder = 5,  CreatedAtUtc = now },
            new NavItem { Key = "users",         Label = "Users Management",           Section = "Management", SortOrder = 5,  CreatedAtUtc = now },
            new NavItem { Key = "departments",   Label = "Department Setup",           Section = "Management", SortOrder = 6,  CreatedAtUtc = now },
            new NavItem { Key = "attendees",     Label = "Attendees / Registrations",  Section = "Management", SortOrder = 7,  CreatedAtUtc = now },
            new NavItem { Key = "check_in",      Label = "Event Check-in",             Section = "Management", SortOrder = 8,  CreatedAtUtc = now },
            new NavItem { Key = "categories",    Label = "Categories Setup",           Section = "Management", SortOrder = 9,  CreatedAtUtc = now },
            new NavItem { Key = "reports",       Label = "Reports & Analytics",        Section = "Analytics",  SortOrder = 10, CreatedAtUtc = now },
            new NavItem { Key = "notifications", Label = "Notifications",              Section = "Analytics",  SortOrder = 11, CreatedAtUtc = now },
            new NavItem { Key = "settings",      Label = "Settings",                   Section = "System",     SortOrder = 12, CreatedAtUtc = now },
        };

        var existingKeys = await _dbContext.NavItems
            .Select(n => n.Key)
            .ToListAsync(cancellationToken);

        var missingNavItems = navItems
            .Where(n => !existingKeys.Contains(n.Key))
            .ToList();

        if (missingNavItems.Count == 0)
            return;

        _dbContext.NavItems.AddRange(missingNavItems);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}

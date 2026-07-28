using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Services;
using NtbEvent.Domain.Entities;
using NtbEvent.Infrastructure.Configuration;
using NtbEvent.Infrastructure.Persistence;
using NtbEvent.Infrastructure.Services;

namespace NtbEvent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Postgres' is missing. Set ConnectionStrings:Postgres in appsettings or environment variables.");
        }

        services.AddSingleton(new DatabaseOptions
        {
            ConnectionString = connectionString
        });

        services.AddHttpContextAccessor();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));
        services.Configure<AppUrlOptions>(configuration.GetSection(AppUrlOptions.SectionName));

        // EF Core — used for CRUD operations (Add, GetById, GetBySlug, Update, Delete)
        services.AddDbContext<NtbEventDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Dapper — used for report/filter list queries (GetAsync)
        services.AddScoped<IDataRepo, DataRepo>();

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IFestivalRepository, FestivalRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICategoryRepository, CategoriesRepository>();
        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IFestivalService, FestivalService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoriesService, CategoriesService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICalendarFilterSettingsRepository, CalendarFilterSettingsRepository>();
        services.AddScoped<ICalendarFilterSettingsService, CalendarFilterSettingsService>();
        services.AddScoped<INavItemRepository, NavItemRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        services.AddScoped<IEventApprovalRepository, EventApprovalRepository>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        // Invitations: repositories, services and supporting QR/email/url helpers.
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
        services.AddScoped<IEventRegistrationService, EventRegistrationService>();
        services.AddScoped<IWorkshopInviteRepository, WorkshopInviteRepository>();
        services.AddScoped<IWorkshopInviteService, WorkshopInviteService>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();
        services.AddSingleton<IAppUrlProvider, AppUrlProvider>();
        services.AddSingleton<IExcelExportService, ExcelExportService>();

        services.AddScoped<IReportsRepository, ReportsRepository>();
        services.AddScoped<IReportsService, ReportsService>();

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}

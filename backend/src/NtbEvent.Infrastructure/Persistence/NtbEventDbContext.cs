using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class NtbEventDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public NtbEventDbContext(DbContextOptions<NtbEventDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Festival> Festivals => Set<Festival>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Categories> Categories => Set<Categories>();
    public DbSet<Tags> Tags => Set<Tags>();
    public DbSet<CatagoriesTags> CatagoriesTags => Set<CatagoriesTags>();

    public DbSet<EventTags> EventTags => Set<EventTags>();

    public DbSet<TagsJn> TagsJn => Set<TagsJn>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<CalendarFilterSettings> CalendarFilterSettings => Set<CalendarFilterSettings>();

    public DbSet<Guest> Guests => Set<Guest>();

    public DbSet<Invitation> Invitations => Set<Invitation>();

    public DbSet<InvitationScan> InvitationScans => Set<InvitationScan>();

    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    public DbSet<WorkshopInvite> WorkshopInvites => Set<WorkshopInvite>();

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    public DbSet<NavItem> NavItems => Set<NavItem>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<EventApprovalRequest> EventApprovalRequests => Set<EventApprovalRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureRefreshTokens(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureTags(modelBuilder);
        ConfigureTagsJn(modelBuilder);
        ConfigureCategoriesTags(modelBuilder);
        ConfigureCalendarFilterSettings(modelBuilder);
        ConfigureGuests(modelBuilder);
        ConfigureInvitations(modelBuilder);
        ConfigureInvitationScans(modelBuilder);
        ConfigureEventRegistrations(modelBuilder);
        ConfigureWorkshopInvites(modelBuilder);
        ConfigureEmailTemplates(modelBuilder);
        ConfigureNavItems(modelBuilder);
        ConfigureUserPermissions(modelBuilder);
        ConfigureEventApprovalRequests(modelBuilder);
        ConfigureNotifications(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(e => e.Slug)
                .HasColumnName("slug")
                .IsRequired();

            entity.HasIndex(e => e.Slug)
                .IsUnique();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired();

            entity.Property(e => e.Summary)
                .HasColumnName("summary")
                .IsRequired();

            entity.Property(e => e.LongDescription)
                .HasColumnName("long_description")
                .IsRequired();

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .IsRequired();

            // Enum stored as lowercase TEXT to match the existing Dapper schema
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<EventType>(v, true));

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<EventLifecycleStatus>(v, true));

            entity.Property(e => e.DateAd)
                .HasColumnName("date_ad")
                .HasColumnType("date");

            entity.Property(e => e.EndDateAd)
                .HasColumnName("end_date_ad")
                .HasColumnType("date");

            entity.Property(e => e.DateBs)
                .HasColumnName("date_bs")
                .IsRequired();

            entity.Property(e => e.EndDateBs)
                .HasColumnName("end_date_bs")
                .IsRequired();

            entity.Property(e => e.Color)
                .HasColumnName("color")
                .IsRequired();

            entity.Property(e => e.Location)
                .HasColumnName("location")
                .IsRequired();

            entity.Property(e => e.Region)
                .HasColumnName("region")
                .IsRequired();

            entity.Property(e => e.Address)
                .HasColumnName("address")
                .IsRequired();

            entity.Property(e => e.Latitude)
                .HasColumnName("latitude");

            entity.Property(e => e.Longitude)
                .HasColumnName("longitude");

            entity.Property(e => e.DateRangeLabel)
                .HasColumnName("date_range_label")
                .IsRequired();

            entity.Property(e => e.DurationLabel)
                .HasColumnName("duration_label")
                .IsRequired();

            entity.Property(e => e.AttendanceLabel)
                .HasColumnName("attendance_label")
                .IsRequired();

            entity.Property(e => e.AttendanceNote)
                .HasColumnName("attendance_note")
                .IsRequired();

            entity.Property(e => e.EntryType)
                .HasColumnName("entry_type")
                .IsRequired();

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .HasColumnType("numeric(12,2)");

            entity.Property(e => e.Rating)
                .HasColumnName("rating")
                .HasColumnType("numeric(4,2)");

            entity.Property(e => e.ReviewsLabel)
                .HasColumnName("reviews_label")
                .IsRequired();

            // Tags stored as JSONB — List<string>
            // var tagsConverter = new ValueConverter<List<string>, string>(
            //     v => JsonSerializer.Serialize(v, JsonOptions),
            //     v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>());

            // var tagsComparer = new ValueComparer<List<string>>(
            //     (a, b) => a != null && b != null && a.SequenceEqual(b),
            //     c => c.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
            //     c => c.ToList());

            // entity.Property(e => e.Tags)
            //     .HasColumnName("tags_json")
            //     .HasColumnType("jsonb")
            //     .HasConversion(tagsConverter, tagsComparer);

           // Images stored as JSON array in a text-compatible column
            var imagesConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => DeserializeImageList(v));

            var imagesComparer = new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                c => c.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                c => c.ToList());

            entity.Property(e => e.Image)
                .HasColumnName("image")
                .HasConversion(imagesConverter, imagesComparer);

            entity.Property(e => e.MapImage)
                .HasColumnName("map_image")
                .IsRequired();

            entity.Property(e => e.Organizer)
                .HasColumnName("organizer")
                .IsRequired();

            entity.Property(e => e.OrganizerSubtitle)
                .HasColumnName("organizer_subtitle")
                .IsRequired();

            entity.Property(e => e.OrganizerVerified)
                .HasColumnName("organizer_verified");

            entity.Property(e => e.OrganizerImageUrl)
                .HasColumnName("organizer_image_url");

            // Highlights stored as JSONB — List<EventHighlight>
            var highlightsConverter = new ValueConverter<List<EventHighlight>, string>(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<EventHighlight>>(v, JsonOptions) ?? new List<EventHighlight>());

            var highlightsComparer = new ValueComparer<List<EventHighlight>>(
                (a, b) => a != null && b != null &&
                          JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                c => JsonSerializer.Serialize(c, JsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<List<EventHighlight>>(
                    JsonSerializer.Serialize(c, JsonOptions), JsonOptions) ?? new List<EventHighlight>());

            entity.Property(e => e.Highlights)
                .HasColumnName("highlights_json")
                .HasColumnType("jsonb")
                .HasConversion(highlightsConverter, highlightsComparer);

            entity.Property(e => e.Featured)
                .HasColumnName("featured");

            entity.Property(e => e.ReadTime)
                .HasColumnName("read_time")
                .IsRequired();

            entity.Property(e => e.RequiresRegistration)
                .HasColumnName("requires_registration")
                .HasDefaultValue(false);

            entity.Property(e => e.RequiresInvitation)
                .HasColumnName("requires_invitation")
                .HasDefaultValue(false);

            entity.Property(e => e.QrOnlyCheckIn)
                .HasColumnName("qr_only_check_in")
                .HasDefaultValue(false);

            entity.Property(e => e.ShowEntryType)
                .HasColumnName("show_entry_type")
                .HasDefaultValue(true);

            entity.Property(e => e.InvitationEmailSubject)
                .HasColumnName("invitation_email_subject");

            entity.Property(e => e.InvitationEmailBodyHtml)
                .HasColumnName("invitation_email_body_html");

            // Enabled self-registration field keys stored as a JSON array
            var selfRegFieldsConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => DeserializeImageList(v));

            var selfRegFieldsComparer = new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                c => c.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                c => c.ToList());

            entity.Property(e => e.SelfRegistrationFields)
                .HasColumnName("self_registration_fields")
                .HasColumnType("jsonb")
                .HasConversion(selfRegFieldsConverter, selfRegFieldsComparer);

            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(e => e.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");
            
entity.Property(e => e.CreatedById)
    .HasColumnName("created_by_id");

entity.Property(e => e.UpdatedById)
    .HasColumnName("updated_by_id");

entity.HasOne(e => e.CreatedBy)
    .WithMany()
    .HasForeignKey(e => e.CreatedById)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.Restrict);

entity.HasOne(e => e.UpdatedBy)
    .WithMany()
    .HasForeignKey(e => e.UpdatedById)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.Restrict);
            // Mirrors the index created in the old Dapper initializer
            entity.HasIndex(e => new { e.Status, e.DateAd })
                .HasDatabaseName("idx_events_status_date");
        });

        modelBuilder.Entity<Festival>(entity =>
        {
            entity.ToTable("festivals");
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(f => f.Slug)
                .HasColumnName("slug")
                .IsRequired();

            entity.HasIndex(f => f.Slug)
                .IsUnique();

            entity.Property(f => f.Title)
                .HasColumnName("title")
                .IsRequired();

            entity.Property(f => f.Summary)
                .HasColumnName("summary")
                .IsRequired();

            entity.Property(f => f.LongDescription)
                .HasColumnName("long_description")
                .IsRequired();

            entity.Property(f => f.Category)
                .HasColumnName("category")
                .IsRequired();

            entity.Property(f => f.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<EventLifecycleStatus>(v, true));

            entity.Property(f => f.DateAd)
                .HasColumnName("date_ad")
                .HasColumnType("date");

            entity.Property(f => f.EndDateAd)
                .HasColumnName("end_date_ad")
                .HasColumnType("date");

            entity.Property(f => f.DateBs)
                .HasColumnName("date_bs")
                .IsRequired();

            entity.Property(f => f.EndDateBs)
                .HasColumnName("end_date_bs")
                .IsRequired();

            entity.Property(f => f.Color)
                .HasColumnName("color")
                .IsRequired();

            entity.Property(f => f.Region)
                .HasColumnName("region")
                .IsRequired();

            entity.Property(f => f.Address)
                .HasColumnName("address")
                .IsRequired();

            entity.Property(f => f.Latitude)
                .HasColumnName("latitude");

            entity.Property(f => f.Longitude)
                .HasColumnName("longitude");

            entity.Property(f => f.DateRangeLabel)
                .HasColumnName("date_range_label")
                .IsRequired();

            entity.Property(f => f.DurationLabel)
                .HasColumnName("duration_label")
                .IsRequired();

            var festivalImagesConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => DeserializeImageList(v));

            var festivalImagesComparer = new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                c => c.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                c => c.ToList());

            entity.Property(f => f.Image)
                .HasColumnName("image")
                .HasConversion(festivalImagesConverter, festivalImagesComparer);

            entity.Property(f => f.Organizer)
                .HasColumnName("organizer")
                .IsRequired()
                .HasDefaultValue(string.Empty);

            entity.Property(f => f.OrganizerSubtitle)
                .HasColumnName("organizer_subtitle")
                .IsRequired()
                .HasDefaultValue(string.Empty);

            entity.Property(f => f.OrganizerVerified)
                .HasColumnName("organizer_verified")
                .HasDefaultValue(true);

            entity.Property(f => f.OrganizerImageUrl)
                .HasColumnName("organizer_image_url");

            var festivalHighlightsConverter = new ValueConverter<List<FestivalHighlight>, string>(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<FestivalHighlight>>(v, JsonOptions) ?? new List<FestivalHighlight>());

            var festivalHighlightsComparer = new ValueComparer<List<FestivalHighlight>>(
                (a, b) => a != null && b != null &&
                          JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                c => JsonSerializer.Serialize(c, JsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<List<FestivalHighlight>>(
                    JsonSerializer.Serialize(c, JsonOptions), JsonOptions) ?? new List<FestivalHighlight>());

            entity.Property(f => f.Highlights)
                .HasColumnName("highlights_json")
                .HasColumnType("jsonb")
                .HasConversion(festivalHighlightsConverter, festivalHighlightsComparer);

            entity.Property(f => f.Featured)
                .HasColumnName("featured");

            entity.Property(f => f.ReadTime)
                .HasColumnName("read_time")
                .IsRequired();

            entity.Property(f => f.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(f => f.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(f => f.CreatedById)
                .HasColumnName("created_by_id");

            entity.Property(f => f.UpdatedById)
                .HasColumnName("updated_by_id");

            entity.HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.UpdatedBy)
                .WithMany()
                .HasForeignKey(f => f.UpdatedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(f => new { f.Status, f.DateAd })
                .HasDatabaseName("idx_festivals_status_date");
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(user => user.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasColumnName("email")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(user => user.NormalizedEmail)
                .HasColumnName("normalized_email")
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(user => user.NormalizedEmail)
                .HasDatabaseName("ux_users_normalized_email")
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(user => user.Department)
                .HasColumnName("department")
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasColumnName("role")
                .HasConversion(
                    value => value.ToString().ToLowerInvariant(),
                    value => Enum.Parse<UserRole>(value, true));

            entity.Property(user => user.IsActive)
                .HasColumnName("is_active");

            entity.Property(user => user.LastLoginAtUtc)
                .HasColumnName("last_login_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(user => user.CreatedByUserId)
                .HasColumnName("created_by_user_id");

            entity.Property(user => user.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(user => user.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");

            entity.HasOne(user => user.CreatedByUser)
                .WithMany(user => user.ManagedUsers)
                .HasForeignKey(user => user.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureRefreshTokens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(token => token.Id);

            entity.Property(token => token.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(token => token.UserId)
                .HasColumnName("user_id");

            entity.Property(token => token.TokenHash)
                .HasColumnName("token_hash")
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(token => token.TokenHash)
                .HasDatabaseName("ux_refresh_tokens_token_hash")
                .IsUnique();

            entity.Property(token => token.TokenFamily)
                .HasColumnName("token_family")
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(token => token.TokenFamily)
                .HasDatabaseName("idx_refresh_tokens_token_family");

            entity.Property(token => token.ExpiresAtUtc)
                .HasColumnName("expires_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(token => token.CreatedByIp)
                .HasColumnName("created_by_ip")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(token => token.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(400)
                .IsRequired();

            entity.Property(token => token.RevokedAtUtc)
                .HasColumnName("revoked_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(token => token.RevokedByIp)
                .HasColumnName("revoked_by_ip")
                .HasMaxLength(128);

            entity.Property(token => token.ReplacedByTokenHash)
                .HasColumnName("replaced_by_token_hash")
                .HasMaxLength(64);

            entity.Property(token => token.RevocationReason)
                .HasColumnName("revocation_reason")
                .HasMaxLength(256);

            entity.Property(token => token.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(token => token.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");

            entity.HasIndex(token => token.UserId)
                .HasDatabaseName("idx_refresh_tokens_user_id");

            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categories>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            entity.Property(c => c.Name)
                .IsRequired();

            entity.Property(c => c.Description)
                .IsRequired();

            entity.Property(c => c.Color)
                .IsRequired();

            entity.Property(c => c.Icon)
                .IsRequired();

            // Enum stored as lowercase TEXT to match the EventType convention
            entity.Property(c => c.Type)
                .HasConversion(
                    v => v.ToString().ToLowerInvariant(),
                    v => Enum.Parse<CategoryType>(v, true))
                .IsRequired();

            entity.Property(c => c.CreatedAtUtc);

            entity.Property(c => c.UpdatedAtUtc);
        });
    }   
    public static void ConfigureTags(ModelBuilder modelBuilder){
        modelBuilder.Entity<Tags>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .ValueGeneratedOnAdd();

            entity.Property(t => t.Name)
                .IsRequired();

            entity.Property(t => t.CreatedAtUtc);

            entity.Property(t => t.UpdatedAtUtc);
        });
    }
    public static void ConfigureTagsJn(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TagsJn>(entity =>
        {
            entity.ToTable("Tags_jn");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .ValueGeneratedOnAdd();

            entity.Property(t => t.TagId)
                .IsRequired();

            entity.Property(t => t.Action)
                .IsRequired();

            entity.Property(t => t.OldName);

            entity.Property(t => t.NewName);

            entity.Property(t => t.ChangedAtUtc)
                .IsRequired();
        });
    }

    public static void ConfigureCategoriesTags(ModelBuilder modelBuilder){
        modelBuilder.Entity<CatagoriesTags>(entity =>
        {
            entity.ToTable("CategoriesTags");
            entity.HasKey(ct => ct.Id);

            entity.Property(ct => ct.Id)
                .ValueGeneratedOnAdd();

            entity.Property(ct => ct.CategoryId)
                .IsRequired();

            entity.Property(ct => ct.TagId)
                .IsRequired();

            entity.HasOne(ct => ct.Category)
                .WithMany()
                .HasForeignKey(ct => ct.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ct => ct.Tag)
                .WithMany()
                .HasForeignKey(ct => ct.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(ct => ct.CreatedAtUtc);

            entity.Property(ct => ct.UpdatedAtUtc);
        });
    }

    public static void ConfigureEventTags(ModelBuilder modelBuilder){
        modelBuilder.Entity<EventTags>(entity =>
        {
            entity.ToTable("EventTags");
            entity.HasKey(et => et.Id);

            entity.Property(et => et.Id)
                .ValueGeneratedOnAdd();

            entity.Property(et => et.EventId)
                .IsRequired();

            entity.Property(et => et.TagId)
                .IsRequired();

            entity.HasOne(et => et.Event)
                .WithMany()
                .HasForeignKey(et => et.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(et => et.Tag)
                .WithMany()
                .HasForeignKey(et => et.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(et => et.CreatedAtUtc);

            entity.Property(et => et.UpdatedAtUtc);
        });
    }

    private static void ConfigureCalendarFilterSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarFilterSettings>(entity =>
        {
            entity.ToTable("calendar_filter_settings");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                .HasColumnName("id")
                .ValueGeneratedNever(); // singleton row; id is always 1

            entity.Property(s => s.ShowCategory)
                .HasColumnName("show_category")
                .HasDefaultValue(true);

            entity.Property(s => s.ShowDateRange)
                .HasColumnName("show_date_range")
                .HasDefaultValue(true);

            entity.Property(s => s.ShowLocation)
                .HasColumnName("show_location")
                .HasDefaultValue(true);

            entity.Property(s => s.ShowPrice)
                .HasColumnName("show_price")
                .HasDefaultValue(true);

            entity.Property(s => s.ShowTags)
                .HasColumnName("show_tags")
                .HasDefaultValue(true);

            entity.Property(s => s.IsSundayHoliday)
                .HasColumnName("is_sunday_holiday")
                .HasDefaultValue(true);

            entity.Property(s => s.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(s => s.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");
        });
    }

    private static void ConfigureGuests(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.ToTable("invitation_guests");
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(g => g.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(g => g.Email)
                .HasColumnName("email")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(g => g.NormalizedEmail)
                .HasColumnName("normalized_email")
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(g => g.NormalizedEmail)
                .HasDatabaseName("ux_invitation_guests_normalized_email")
                .IsUnique();

            entity.Property(g => g.Phone)
                .HasColumnName("phone")
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(g => g.Organization)
                .HasColumnName("organization")
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(g => g.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(g => g.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");
        });
    }

    private static void ConfigureInvitations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.ToTable("event_invitations");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(i => i.EventId)
                .HasColumnName("event_id");

            entity.Property(i => i.GuestId)
                .HasColumnName("guest_id");

            entity.Property(i => i.Token)
                .HasColumnName("token")
                .HasMaxLength(80)
                .IsRequired();

            entity.HasIndex(i => i.Token)
                .HasDatabaseName("ux_event_invitations_token")
                .IsUnique();

            entity.HasIndex(i => new { i.EventId, i.GuestId })
                .HasDatabaseName("ux_event_invitations_event_guest")
                .IsUnique();

            entity.Property(i => i.Status)
                .HasColumnName("status")
                .HasConversion(
                    value => value.ToString().ToLowerInvariant(),
                    value => Enum.Parse<NtbEvent.Domain.Enums.InvitationStatus>(value, true))
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(i => i.InvitedByUserId)
                .HasColumnName("invited_by_user_id");

            entity.Property(i => i.SentAtUtc)
                .HasColumnName("sent_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(i => i.ExpiresAtUtc)
                .HasColumnName("expires_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(i => i.VerifiedAtUtc)
                .HasColumnName("verified_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(i => i.VerifiedByUserId)
                .HasColumnName("verified_by_user_id");

            entity.Property(i => i.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(i => i.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");

            entity.HasOne(i => i.Event)
                .WithMany()
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Guest)
                .WithMany(g => g.Invitations)
                .HasForeignKey(i => i.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.InvitedBy)
                .WithMany()
                .HasForeignKey(i => i.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.VerifiedBy)
                .WithMany()
                .HasForeignKey(i => i.VerifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureInvitationScans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvitationScan>(entity =>
        {
            entity.ToTable("invitation_scans");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(s => s.InvitationId)
                .HasColumnName("invitation_id");

            entity.Property(s => s.ScannedByUserId)
                .HasColumnName("scanned_by_user_id");

            entity.Property(s => s.Result)
                .HasColumnName("result")
                .HasConversion(
                    value => value.ToString().ToLowerInvariant(),
                    value => Enum.Parse<NtbEvent.Domain.Enums.ScanResult>(value, true))
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(s => s.ScannedAtUtc)
                .HasColumnName("scanned_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(s => s.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(s => s.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");

            entity.HasIndex(s => s.InvitationId)
                .HasDatabaseName("idx_invitation_scans_invitation_id");

            entity.HasOne(s => s.Invitation)
                .WithMany(i => i.Scans)
                .HasForeignKey(s => s.InvitationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.ScannedBy)
                .WithMany()
                .HasForeignKey(s => s.ScannedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEventRegistrations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.ToTable("event_registrations");
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(r => r.EventId)
                .HasColumnName("event_id");

            entity.Property(r => r.GuestId)
                .HasColumnName("guest_id");

            entity.HasIndex(r => new { r.EventId, r.GuestId })
                .HasDatabaseName("ux_event_registrations_event_guest")
                .IsUnique();

            entity.Property(r => r.Status)
                .HasColumnName("status")
                .HasConversion(
                    value => value.ToString().ToLowerInvariant(),
                    value => Enum.Parse<NtbEvent.Domain.Enums.RegistrationStatus>(value, true))
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(r => r.RequestedAtUtc)
                .HasColumnName("requested_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(r => r.ReviewedByUserId)
                .HasColumnName("reviewed_by_user_id");

            entity.Property(r => r.ReviewedAtUtc)
                .HasColumnName("reviewed_at_utc")
                .HasColumnType("timestamptz");

            // Submitted values for the event's enabled optional self-registration fields
            var additionalFieldsConverter = new ValueConverter<Dictionary<string, string>, string>(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions) ?? new Dictionary<string, string>());

            var additionalFieldsComparer = new ValueComparer<Dictionary<string, string>>(
                (a, b) => a != null && b != null &&
                          JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                c => JsonSerializer.Serialize(c, JsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<string, string>>(
                    JsonSerializer.Serialize(c, JsonOptions), JsonOptions) ?? new Dictionary<string, string>());

            entity.Property(r => r.AdditionalFields)
                .HasColumnName("additional_fields")
                .HasColumnType("jsonb")
                .HasConversion(additionalFieldsConverter, additionalFieldsComparer);

            entity.Property(r => r.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(r => r.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");

            entity.HasOne(r => r.Event)
                .WithMany()
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Guest)
                .WithMany(g => g.Registrations)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ReviewedBy)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureWorkshopInvites(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkshopInvite>(entity =>
        {
            entity.ToTable("workshop_invites");
            entity.HasKey(w => w.Id);

            entity.Property(w => w.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(w => w.EventId)
                .HasColumnName("event_id");

            entity.Property(w => w.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(w => w.Email)
                .HasColumnName("email")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(w => w.NormalizedEmail)
                .HasColumnName("normalized_email")
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(w => new { w.EventId, w.NormalizedEmail })
                .HasDatabaseName("ux_workshop_invites_event_email")
                .IsUnique();

            entity.Property(w => w.Phone)
                .HasColumnName("phone")
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(w => w.Organization)
                .HasColumnName("organization")
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(w => w.Status)
                .HasColumnName("status")
                .HasConversion(
                    value => value.ToString().ToLowerInvariant(),
                    value => Enum.Parse<NtbEvent.Domain.Enums.WorkshopInviteStatus>(value, true))
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(w => w.SentAtUtc)
                .HasColumnName("sent_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(w => w.Token)
                .HasColumnName("token")
                .HasMaxLength(80)
                .IsRequired();

            entity.HasIndex(w => w.Token)
                .HasDatabaseName("ux_workshop_invites_token")
                .IsUnique();

            entity.Property(w => w.VerifiedAtUtc)
                .HasColumnName("verified_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(w => w.VerifiedByUserId)
                .HasColumnName("verified_by_user_id");

            entity.Property(w => w.WorkshopDateAd)
                .HasColumnName("workshop_date_ad")
                .HasColumnType("date");

            entity.Property(w => w.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(w => w.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");

            entity.HasOne(w => w.VerifiedBy)
                .WithMany()
                .HasForeignKey(w => w.VerifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(w => w.Event)
                .WithMany()
                .HasForeignKey(w => w.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureEmailTemplates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.ToTable("email_templates");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(t => t.Type)
                .HasColumnName("type")
                .HasMaxLength(40)
                .IsRequired();

            entity.HasIndex(t => t.Type)
                .HasDatabaseName("ux_email_templates_type")
                .IsUnique();

            entity.Property(t => t.Subject)
                .HasColumnName("subject")
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(t => t.BodyHtml)
                .HasColumnName("body_html")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(t => t.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz");

            entity.Property(t => t.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz");
        });
    }

    private static void ConfigureNavItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NavItem>(entity =>
        {
            entity.ToTable("nav_items");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(n => n.Key).HasColumnName("key").HasMaxLength(50).IsRequired();
            entity.HasIndex(n => n.Key).HasDatabaseName("ux_nav_items_key").IsUnique();
            entity.Property(n => n.Label).HasColumnName("label").HasMaxLength(100).IsRequired();
            entity.Property(n => n.Section).HasColumnName("section").HasMaxLength(50).IsRequired();
            entity.Property(n => n.SortOrder).HasColumnName("sort_order");
            entity.Property(n => n.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamptz");
        });
    }

    private static void ConfigureEventApprovalRequests(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventApprovalRequest>(entity =>
        {
            entity.ToTable("event_approval_requests");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(r => r.EventId).HasColumnName("event_id");
            entity.Property(r => r.RequestedByUserId).HasColumnName("requested_by_user_id");
            entity.Property(r => r.ReviewedByUserId).HasColumnName("reviewed_by_user_id");

            entity.Property(r => r.Action)
                .HasColumnName("action")
                .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<ApprovalAction>(v, true))
                .HasMaxLength(20);

            entity.Property(r => r.Status)
                .HasColumnName("status")
                .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<ApprovalStatus>(v, true))
                .HasMaxLength(20);

            entity.Property(r => r.OriginalStatus)
                .HasColumnName("original_status")
                .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<EventLifecycleStatus>(v, true))
                .HasMaxLength(30);

            entity.Property(r => r.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(r => r.RequestedAtUtc).HasColumnName("requested_at_utc").HasColumnType("timestamptz");
            entity.Property(r => r.ReviewedAtUtc).HasColumnName("reviewed_at_utc").HasColumnType("timestamptz");

            entity.HasIndex(r => r.Status).HasDatabaseName("idx_event_approvals_status");

            entity.HasOne(r => r.Event)
                .WithMany(e => e.ApprovalRequests)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.RequestedBy)
                .WithMany()
                .HasForeignKey(r => r.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReviewedBy)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureUserPermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("user_permissions");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(p => p.UserId).HasColumnName("user_id");
            entity.Property(p => p.NavItemId).HasColumnName("nav_item_id");
            entity.Property(p => p.CanView).HasColumnName("can_view").HasDefaultValue(false);
            entity.Property(p => p.CanCreate).HasColumnName("can_create").HasDefaultValue(false);
            entity.Property(p => p.CanUpdate).HasColumnName("can_update").HasDefaultValue(false);
            entity.Property(p => p.CanDelete).HasColumnName("can_delete").HasDefaultValue(false);
            entity.Property(p => p.NeedsApproval).HasColumnName("needs_approval").HasDefaultValue(false);
            entity.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamptz");
            entity.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamptz");

            entity.HasIndex(p => new { p.UserId, p.NavItemId })
                .HasDatabaseName("ux_user_permissions_user_navitem")
                .IsUnique();

            entity.HasOne(p => p.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.NavItem)
                .WithMany()
                .HasForeignKey(p => p.NavItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id)
                .HasColumnName("id")
                .UseIdentityByDefaultColumn();

            entity.Property(n => n.UserId).HasColumnName("user_id");
            entity.Property(n => n.Type).HasColumnName("type").HasMaxLength(50);
            entity.Property(n => n.Title).HasColumnName("title").HasMaxLength(200);
            entity.Property(n => n.Message).HasColumnName("message").HasMaxLength(500);
            entity.Property(n => n.Link).HasColumnName("link").HasMaxLength(300);
            entity.Property(n => n.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(n => n.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamptz");

            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(n => n.UserId).HasDatabaseName("idx_notifications_user_id");
            entity.HasIndex(n => new { n.UserId, n.IsRead }).HasDatabaseName("idx_notifications_user_unread");
        });
    }

    private static List<string> DeserializeImageList(string v)
    {
        if (string.IsNullOrWhiteSpace(v))
            return [];

        var trimmed = v.Trim();
        if (trimmed.StartsWith('['))
        {
            try { return JsonSerializer.Deserialize<List<string>>(trimmed, JsonOptions) ?? []; }
            catch (JsonException) { }
        }

        return [trimmed];
    }
}

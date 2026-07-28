using System.Security.Cryptography;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.EmailTemplates;
using NtbEvent.Application.WorkshopInvites.Dtos;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class WorkshopInviteService : IWorkshopInviteService
{
    private readonly IWorkshopInviteRepository _workshopInviteRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IEmailService _emailService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly IEmailTemplateService _emailTemplateService;

    public WorkshopInviteService(
        IWorkshopInviteRepository workshopInviteRepository,
        IEventRepository eventRepository,
        IEmailService emailService,
        IQrCodeService qrCodeService,
        IAppUrlProvider appUrlProvider,
        IEmailTemplateService emailTemplateService)
    {
        _workshopInviteRepository = workshopInviteRepository;
        _eventRepository = eventRepository;
        _emailService = emailService;
        _qrCodeService = qrCodeService;
        _appUrlProvider = appUrlProvider;
        _emailTemplateService = emailTemplateService;
    }

    public async Task<int> ImportAsync(long eventId, ImportWorkshopInviteesRequest request, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanManageEvent(@event, callerUserId, isSuperAdmin);

        var existing = await _workshopInviteRepository.GetNormalizedEmailsByEventAsync(eventId, cancellationToken);
        var now = DateTime.UtcNow;
        var seenInBatch = new HashSet<string>();

        var toInsert = new List<WorkshopInvite>();
        foreach (var row in request.Rows)
        {
            var email = (row.Email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                continue;
            }

            var normalizedEmail = email.ToUpperInvariant();
            if (!existing.Add(normalizedEmail) || !seenInBatch.Add(normalizedEmail))
            {
                continue;
            }

            toInsert.Add(new WorkshopInvite
            {
                EventId = eventId,
                FullName = row.FullName.Trim(),
                Email = email,
                NormalizedEmail = normalizedEmail,
                Phone = row.Phone?.Trim() ?? string.Empty,
                Organization = row.Organization?.Trim() ?? string.Empty,
                Status = WorkshopInviteStatus.Pending,
                Token = GenerateToken(),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        if (toInsert.Count > 0)
        {
            await _workshopInviteRepository.AddRangeAsync(toInsert, cancellationToken);
        }

        return toInsert.Count;
    }

    public async Task<IReadOnlyList<WorkshopInviteDto>> GetForEventAsync(long eventId, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanManageEvent(@event, callerUserId, isSuperAdmin);

        var invites = await _workshopInviteRepository.GetByEventAsync(eventId, cancellationToken);
        return invites.Select(invite => Map(invite, @event)).ToList();
    }

    public async Task<IReadOnlyList<WorkshopInviteDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invites = await _workshopInviteRepository.GetAllAsync(cancellationToken);
        return invites.Select(invite => Map(invite, invite.Event)).ToList();
    }

    public async Task<WorkshopInviteDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var invite = await _workshopInviteRepository.GetByIdAsync(id, cancellationToken);
        return invite is null ? null : Map(invite, invite.Event);
    }

    public async Task<WorkshopInviteDto?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var invite = await _workshopInviteRepository.GetByTokenAsync(ExtractToken(token), cancellationToken);
        if (invite is null || invite.Event?.QrOnlyCheckIn == true)
        {
            return null;
        }

        return Map(invite, invite.Event);
    }

    public async Task<byte[]?> GetQrPngAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invite = await _workshopInviteRepository.GetByIdAsync(id, cancellationToken);
        if (invite is null)
        {
            return null;
        }

        EnsureCanManageEvent(invite.Event, callerUserId, isSuperAdmin);

        return _qrCodeService.GeneratePng(_appUrlProvider.BuildWorkshopInviteUrl(invite.Token));
    }

    public async Task<WorkshopInviteDto?> SendAsync(long id, SendWorkshopInviteRequest request, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invite = await _workshopInviteRepository.GetByIdAsync(id, cancellationToken);
        if (invite is null)
        {
            return null;
        }

        EnsureCanManageEvent(invite.Event, callerUserId, isSuperAdmin);

        if (string.IsNullOrEmpty(invite.Token))
        {
            invite.Token = GenerateToken();
        }

        if (!DateTime.TryParse(request.WorkshopDate, out var workshopDate))
        {
            throw new ArgumentException("Workshop date must be a valid date.", nameof(request));
        }

        var inviteUrl = _appUrlProvider.BuildWorkshopInviteUrl(invite.Token);
        var qrPng = _qrCodeService.GeneratePng(inviteUrl);
        var workshopDateLabel = FormatEmailDate(workshopDate);
        var qrOnly = invite.Event?.QrOnlyCheckIn == true;

        var inviteUrlLine = qrOnly
            ? string.Empty
            : $"""<p style="text-align:center;font-size:12px;color:#94a3b8;margin:14px 0 0;">Or open: {inviteUrl}</p>""";
        var qrCodeImage = """<img src="cid:workshop-invite-qr" alt="Workshop invite QR code" width="220" height="220" style="display:block;margin:0 auto;border-radius:8px;" />""";

        var (subject, htmlBody) = await _emailTemplateService.RenderAsync(
            EmailTemplateTypes.WorkshopInvite,
            new Dictionary<string, string>
            {
                ["FullName"] = System.Net.WebUtility.HtmlEncode(invite.FullName),
                ["WorkshopDate"] = System.Net.WebUtility.HtmlEncode(workshopDateLabel),
                ["InviteUrlLine"] = inviteUrlLine,
                ["QrCodeImage"] = qrCodeImage
            },
            cancellationToken: cancellationToken);

        var message = new EmailMessage
        {
            ToEmail = invite.Email,
            ToName = invite.FullName,
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = HtmlToPlainText.Convert(htmlBody),
            InlineImagePng = qrPng,
            InlineImageContentId = "workshop-invite-qr",
            InlineImageFileName = "workshop-invite-qr.png"
        };

        await _emailService.SendAsync(message, cancellationToken);

        var now = DateTime.UtcNow;
        if (invite.Status != WorkshopInviteStatus.Verified)
        {
            invite.Status = WorkshopInviteStatus.Sent;
        }
        invite.SentAtUtc = now;
        invite.WorkshopDateAd = workshopDate.Date;
        invite.UpdatedAtUtc = now;
        await _workshopInviteRepository.UpdateAsync(invite, cancellationToken);

        return Map(invite, invite.Event);
    }

    public async Task<WorkshopInviteScanResultDto> ScanAsync(string token, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invite = await _workshopInviteRepository.GetByTokenAsync(ExtractToken(token), cancellationToken);
        if (invite is null)
        {
            return new WorkshopInviteScanResultDto
            {
                Result = "invalid",
                Message = "No workshop invite matches this QR code.",
                CanVerify = false
            };
        }

        EnsureCanManageEvent(invite.Event, callerUserId, isSuperAdmin);

        var (result, message, canVerify) = Evaluate(invite);
        return new WorkshopInviteScanResultDto
        {
            Result = result,
            Message = message,
            CanVerify = canVerify,
            Invite = Map(invite, invite.Event)
        };
    }

    public async Task<WorkshopInviteScanResultDto?> VerifyAsync(long id, long verifiedByUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invite = await _workshopInviteRepository.GetByIdAsync(id, cancellationToken);
        if (invite is null)
        {
            return null;
        }

        EnsureCanManageEvent(invite.Event, verifiedByUserId, isSuperAdmin);

        var (result, message, canVerify) = Evaluate(invite);
        if (!canVerify)
        {
            return new WorkshopInviteScanResultDto
            {
                Result = result,
                Message = message,
                CanVerify = false,
                Invite = Map(invite, invite.Event)
            };
        }

        var now = DateTime.UtcNow;
        invite.Status = WorkshopInviteStatus.Verified;
        invite.VerifiedAtUtc = now;
        invite.VerifiedByUserId = verifiedByUserId;
        invite.UpdatedAtUtc = now;
        await _workshopInviteRepository.UpdateAsync(invite, cancellationToken);

        return new WorkshopInviteScanResultDto
        {
            Result = "verified",
            Message = $"{invite.FullName} checked in successfully. This QR is now used.",
            CanVerify = false,
            Invite = Map(invite, invite.Event)
        };
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Only the event's creator or a SuperAdmin may invite/check-in guests for it —
    /// prevents one Admin from managing an event they didn't create.
    /// </summary>
    private static void EnsureCanManageEvent(Event @event, long callerUserId, bool isSuperAdmin)
    {
        if (isSuperAdmin || @event.CreatedById == callerUserId)
        {
            return;
        }

        throw new UnauthorizedAccessException("Only the event's creator or a SuperAdmin can manage invitations for this event.");
    }

    private static (string Result, string Message, bool CanVerify) Evaluate(WorkshopInvite invite)
    {
        if (invite.Status == WorkshopInviteStatus.Verified)
        {
            return ("alreadyused", $"This QR was already used to check in {invite.FullName}. Entry refused.", false);
        }

        return ("previewed", "Valid workshop invite. Confirm to check the guest in.", true);
    }

    private WorkshopInviteDto Map(WorkshopInvite invite, Domain.Entities.Event? @event)
    {
        var inviteUrl = string.IsNullOrEmpty(invite.Token)
            ? string.Empty
            : _appUrlProvider.BuildWorkshopInviteUrl(invite.Token);

        return new WorkshopInviteDto
        {
            Id = invite.Id,
            EventId = invite.EventId,
            EventTitle = @event?.Title ?? string.Empty,
            EventDateAd = invite.WorkshopDateAd ?? @event?.DateAd,
            EventLocation = string.IsNullOrWhiteSpace(@event?.Location) ? (@event?.Region ?? string.Empty) : @event.Location,
            FullName = invite.FullName,
            Email = invite.Email,
            Phone = invite.Phone,
            Organization = invite.Organization,
            Status = invite.Status.ToString().ToLowerInvariant(),
            SentAtUtc = invite.SentAtUtc,
            CreatedAtUtc = invite.CreatedAtUtc,
            Token = invite.Token,
            InviteUrl = inviteUrl,
            QrDataUri = string.IsNullOrEmpty(invite.Token) ? string.Empty : _qrCodeService.GenerateDataUri(inviteUrl),
            VerifiedAtUtc = invite.VerifiedAtUtc
        };
    }

    private static string GenerateToken()
    {
        // 160-bit unguessable token, URL-safe (lowercase hex).
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(20)).ToLowerInvariant();
    }

    /// <summary>
    /// Accepts a raw token or a full invite URL (e.g. .../workshop-invite/&lt;token&gt;)
    /// and returns the bare token so manual entry and URL-encoded QRs both work.
    /// </summary>
    private static string ExtractToken(string raw)
    {
        var value = (raw ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return value;
        }

        const string marker = "/workshop-invite/";
        var markerIndex = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex >= 0)
        {
            value = value[(markerIndex + marker.Length)..];
        }

        var cut = value.IndexOfAny(['?', '#', '/']);
        if (cut >= 0)
        {
            value = value[..cut];
        }

        return value.Trim();
    }

    private static string FormatEmailDate(DateTime date) => $"{date.Day} {date:MMMM}";
}

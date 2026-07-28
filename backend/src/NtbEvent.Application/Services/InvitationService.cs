using System.Security.Cryptography;
using NtbEvent.Application.Common;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.EmailTemplates;
using NtbEvent.Application.Invitations.Dtos;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Application.Services;

public sealed class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IQrCodeService _qrCodeService;
    private readonly IEmailService _emailService;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly IEmailTemplateService _emailTemplateService;

    public InvitationService(
        IInvitationRepository invitationRepository,
        IGuestRepository guestRepository,
        IEventRepository eventRepository,
        IQrCodeService qrCodeService,
        IEmailService emailService,
        IAppUrlProvider appUrlProvider,
        IEmailTemplateService emailTemplateService)
    {
        _invitationRepository = invitationRepository;
        _guestRepository = guestRepository;
        _eventRepository = eventRepository;
        _qrCodeService = qrCodeService;
        _emailService = emailService;
        _appUrlProvider = appUrlProvider;
        _emailTemplateService = emailTemplateService;
    }

    public async Task<InvitationDto> InviteAsync(
        long eventId,
        InviteGuestRequest request,
        long invitedByUserId,
        bool isSuperAdmin,
        CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanManageEvent(@event, invitedByUserId, isSuperAdmin);

        var email = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Guest email is required.");
        }

        var normalizedEmail = email.ToUpperInvariant();
        var now = DateTime.UtcNow;

        // Upsert the guest (one row per unique email — normalized).
        var guest = await _guestRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (guest is null)
        {
            guest = new Guest
            {
                FullName = request.FullName.Trim(),
                Email = email,
                NormalizedEmail = normalizedEmail,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Organization = request.Organization?.Trim() ?? string.Empty,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            guest = await _guestRepository.AddAsync(guest, cancellationToken);
        }
        else
        {
            // Refresh details with the latest values provided.
            guest.FullName = request.FullName.Trim();
            guest.Phone = request.Phone?.Trim() ?? guest.Phone;
            guest.Organization = request.Organization?.Trim() ?? guest.Organization;
            guest.UpdatedAtUtc = now;
            await _guestRepository.UpdateAsync(guest, cancellationToken);
        }

        // Prevent duplicate active invitations to the same event. A cancelled
        // invitation is reactivated (reusing the row) with a fresh token.
        var existing = await _invitationRepository.GetByEventAndGuestAsync(eventId, guest.Id, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status != InvitationStatus.Cancelled)
            {
                throw new InvalidOperationException("This guest has already been invited to this event.");
            }

            existing.Token = GenerateToken();
            existing.Status = InvitationStatus.Pending;
            existing.InvitedByUserId = invitedByUserId;
            existing.ExpiresAtUtc = request.ExpiresAtUtc?.ToUniversalTime() ?? ResolveDefaultExpiry(@event);
            existing.SentAtUtc = null;
            existing.VerifiedAtUtc = null;
            existing.VerifiedByUserId = null;
            existing.UpdatedAtUtc = now;
            await _invitationRepository.UpdateAsync(existing, cancellationToken);

            await SendInvitationEmailAsync(existing, @event, guest, cancellationToken);
            return Map(existing, @event, guest);
        }

        var invitation = new Invitation
        {
            EventId = eventId,
            GuestId = guest.Id,
            Token = GenerateToken(),
            Status = InvitationStatus.Pending,
            InvitedByUserId = invitedByUserId,
            ExpiresAtUtc = request.ExpiresAtUtc?.ToUniversalTime() ?? ResolveDefaultExpiry(@event),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        invitation = await _invitationRepository.AddAsync(invitation, cancellationToken);

        await SendInvitationEmailAsync(invitation, @event, guest, cancellationToken);

        return Map(invitation, @event, guest);
    }

    public async Task<IReadOnlyList<InvitationDto>> GetForEventAsync(long eventId, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanManageEvent(@event, callerUserId, isSuperAdmin);

        var invitations = await _invitationRepository.GetByEventAsync(eventId, cancellationToken);
        return invitations.Select(invitation => Map(invitation, invitation.Event, invitation.Guest)).ToList();
    }

    public async Task<IReadOnlyList<InvitationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invitations = await _invitationRepository.GetAllAsync(cancellationToken);
        return invitations.Select(invitation => Map(invitation, invitation.Event, invitation.Guest)).ToList();
    }

    public async Task<InvitationDto?> GetByIdAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id, cancellationToken);
        if (invitation is null)
        {
            return null;
        }

        EnsureCanManageEvent(invitation.Event, callerUserId, isSuperAdmin);
        return Map(invitation, invitation.Event, invitation.Guest);
    }

    public async Task<InvitationDto?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(ExtractToken(token), cancellationToken);
        return invitation is null ? null : Map(invitation, invitation.Event, invitation.Guest);
    }

    public async Task<InvitationDto?> ResendAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id, cancellationToken);
        if (invitation is null)
        {
            return null;
        }

        EnsureCanManageEvent(invitation.Event, callerUserId, isSuperAdmin);

        if (invitation.Status == InvitationStatus.Verified)
        {
            throw new InvalidOperationException("This invitation has already been used and cannot be resent.");
        }

        if (invitation.Status == InvitationStatus.Cancelled)
        {
            throw new InvalidOperationException("This invitation was cancelled and cannot be resent.");
        }

        await SendInvitationEmailAsync(invitation, invitation.Event, invitation.Guest, cancellationToken);
        return Map(invitation, invitation.Event, invitation.Guest);
    }

    public async Task<ScanResultDto> ScanAsync(string token, long scannedByUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(ExtractToken(token), cancellationToken);
        if (invitation is null)
        {
            return new ScanResultDto
            {
                Result = "invalid",
                Message = "No invitation matches this QR code.",
                CanVerify = false
            };
        }

        EnsureCanManageEvent(invitation.Event, scannedByUserId, isSuperAdmin);

        var (result, message, canVerify) = Evaluate(invitation);

        await _invitationRepository.AddScanAsync(new InvitationScan
        {
            InvitationId = invitation.Id,
            ScannedByUserId = scannedByUserId,
            Result = result,
            ScannedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return new ScanResultDto
        {
            Result = result.ToString().ToLowerInvariant(),
            Message = message,
            CanVerify = canVerify,
            Invitation = Map(invitation, invitation.Event, invitation.Guest)
        };
    }

    public async Task<ScanResultDto?> VerifyAsync(long id, long verifiedByUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id, cancellationToken);
        if (invitation is null)
        {
            return null;
        }

        EnsureCanManageEvent(invitation.Event, verifiedByUserId, isSuperAdmin);

        var (result, message, canVerify) = Evaluate(invitation);
        if (!canVerify)
        {
            // Already used / expired / cancelled — refuse and report current state.
            await _invitationRepository.AddScanAsync(new InvitationScan
            {
                InvitationId = invitation.Id,
                ScannedByUserId = verifiedByUserId,
                Result = result,
                ScannedAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            }, cancellationToken);

            return new ScanResultDto
            {
                Result = result.ToString().ToLowerInvariant(),
                Message = message,
                CanVerify = false,
                Invitation = Map(invitation, invitation.Event, invitation.Guest)
            };
        }

        var now = DateTime.UtcNow;
        invitation.Status = InvitationStatus.Verified;
        invitation.VerifiedAtUtc = now;
        invitation.VerifiedByUserId = verifiedByUserId;
        invitation.UpdatedAtUtc = now;
        await _invitationRepository.UpdateAsync(invitation, cancellationToken);

        await _invitationRepository.AddScanAsync(new InvitationScan
        {
            InvitationId = invitation.Id,
            ScannedByUserId = verifiedByUserId,
            Result = ScanResult.Verified,
            ScannedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        }, cancellationToken);

        return new ScanResultDto
        {
            Result = "verified",
            Message = $"{invitation.Guest.FullName} checked in successfully. This QR is now used.",
            CanVerify = false,
            Invitation = Map(invitation, invitation.Event, invitation.Guest)
        };
    }

    public async Task<bool> CancelAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id, cancellationToken);
        if (invitation is null)
        {
            return false;
        }

        EnsureCanManageEvent(invitation.Event, callerUserId, isSuperAdmin);

        if (invitation.Status == InvitationStatus.Cancelled)
        {
            return true;
        }

        invitation.Status = InvitationStatus.Cancelled;
        invitation.UpdatedAtUtc = DateTime.UtcNow;
        await _invitationRepository.UpdateAsync(invitation, cancellationToken);
        return true;
    }

    public async Task<byte[]?> GetQrPngAsync(long id, long callerUserId, bool isSuperAdmin, CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByIdAsync(id, cancellationToken);
        if (invitation is null)
        {
            return null;
        }

        EnsureCanManageEvent(invitation.Event, callerUserId, isSuperAdmin);

        return _qrCodeService.GeneratePng(_appUrlProvider.BuildInviteUrl(invitation.Token));
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

    private async Task SendInvitationEmailAsync(
        Invitation invitation,
        Event @event,
        Guest guest,
        CancellationToken cancellationToken)
    {
        var inviteUrl = _appUrlProvider.BuildInviteUrl(invitation.Token);
        var qrPng = _qrCodeService.GeneratePng(inviteUrl);

        var location = string.IsNullOrWhiteSpace(@event.Location) ? @event.Region : @event.Location;
        var expiryLine = invitation.ExpiresAtUtc is { } expiry
            ? $"""<p style="color:#64748b;font-size:13px;margin:4px 0 0;">Valid until {expiry:dddd, MMMM dd, yyyy}.</p>"""
            : string.Empty;
        var qrCodeImage = """<img src="cid:invitation-qr" alt="Invitation QR code" width="220" height="220" style="display:block;margin:0 auto;border-radius:8px;" />""";

        var (subject, htmlBody) = await _emailTemplateService.RenderAsync(
            EmailTemplateTypes.EventInvitation,
            new Dictionary<string, string>
            {
                ["FullName"] = System.Net.WebUtility.HtmlEncode(guest.FullName),
                ["EventTitle"] = System.Net.WebUtility.HtmlEncode(@event.Title),
                ["EventSummary"] = System.Net.WebUtility.HtmlEncode(@event.Summary),
                ["EventDate"] = @event.DateAd.ToString("dddd, MMMM dd, yyyy"),
                ["EventLocation"] = System.Net.WebUtility.HtmlEncode(location),
                ["InviteUrl"] = inviteUrl,
                ["ExpiryLine"] = expiryLine,
                ["QrCodeImage"] = qrCodeImage
            },
            subjectOverride: @event.InvitationEmailSubject,
            bodyHtmlOverride: @event.InvitationEmailBodyHtml,
            cancellationToken: cancellationToken);

        var message = new EmailMessage
        {
            ToEmail = guest.Email,
            ToName = guest.FullName,
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = HtmlToPlainText.Convert(htmlBody),
            InlineImagePng = qrPng,
            InlineImageContentId = "invitation-qr",
            InlineImageFileName = "invitation-qr.png"
        };

        await _emailService.SendAsync(message, cancellationToken);

        var now = DateTime.UtcNow;
        invitation.Status = InvitationStatus.Sent;
        invitation.SentAtUtc = now;
        invitation.UpdatedAtUtc = now;
        await _invitationRepository.UpdateAsync(invitation, cancellationToken);
    }

    private static (ScanResult Result, string Message, bool CanVerify) Evaluate(Invitation invitation)
    {
        if (invitation.Status == InvitationStatus.Verified)
        {
            return (ScanResult.AlreadyUsed,
                $"This QR was already used to check in {invitation.Guest.FullName}. Entry refused.", false);
        }

        if (invitation.Status == InvitationStatus.Cancelled)
        {
            return (ScanResult.Cancelled, "This invitation was cancelled.", false);
        }

        if (invitation.ExpiresAtUtc is { } expiry && expiry < DateTime.UtcNow)
        {
            return (ScanResult.Expired, "This invitation has expired.", false);
        }

        return (ScanResult.Previewed, "Valid invitation. Confirm to check the guest in.", true);
    }

    private InvitationDto Map(Invitation invitation, Event? @event, Guest? guest)
    {
        return new InvitationDto
        {
            Id = invitation.Id,
            EventId = invitation.EventId,
            EventTitle = @event?.Title ?? string.Empty,
            EventDateAd = @event?.DateAd,
            EventLocation = string.IsNullOrWhiteSpace(@event?.Location) ? (@event?.Region ?? string.Empty) : @event.Location,
            GuestId = invitation.GuestId,
            GuestName = guest?.FullName ?? string.Empty,
            GuestEmail = guest?.Email ?? string.Empty,
            GuestPhone = guest?.Phone ?? string.Empty,
            GuestOrganization = guest?.Organization ?? string.Empty,
            Token = invitation.Token,
            Status = invitation.Status.ToString().ToLowerInvariant(),
            InviteUrl = _appUrlProvider.BuildInviteUrl(invitation.Token),
            QrDataUri = _qrCodeService.GenerateDataUri(_appUrlProvider.BuildInviteUrl(invitation.Token)),
            SentAtUtc = invitation.SentAtUtc,
            ExpiresAtUtc = invitation.ExpiresAtUtc,
            VerifiedAtUtc = invitation.VerifiedAtUtc,
            CreatedAtUtc = invitation.CreatedAtUtc
        };
    }

    private static DateTime ResolveDefaultExpiry(Event @event)
    {
        // Allow check-in until the end of the event day (UTC).
        var endDate = @event.EndDateAd == default ? @event.DateAd : @event.EndDateAd;
        if (endDate == default)
        {
            return DateTime.UtcNow.AddDays(30);
        }

        return DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
    }

    private static string GenerateToken()
    {
        // 160-bit unguessable token, URL-safe (lowercase hex).
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(20)).ToLowerInvariant();
    }

    /// <summary>
    /// Accepts a raw token or a full invite URL (e.g. .../invite/&lt;token&gt;) and
    /// returns the bare token so manual entry and URL-encoded QRs both work.
    /// </summary>
    private static string ExtractToken(string raw)
    {
        var value = (raw ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return value;
        }

        const string marker = "/invite/";
        var markerIndex = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex >= 0)
        {
            value = value[(markerIndex + marker.Length)..];
        }

        // Strip any trailing query string / fragment / slash.
        var cut = value.IndexOfAny(['?', '#', '/']);
        if (cut >= 0)
        {
            value = value[..cut];
        }

        return value.Trim();
    }

}

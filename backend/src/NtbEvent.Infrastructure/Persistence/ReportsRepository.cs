using Microsoft.EntityFrameworkCore;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Reports.Dtos;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class ReportsRepository : IReportsRepository
{
    private readonly NtbEventDbContext _db;

    public ReportsRepository(NtbEventDbContext db) => _db = db;

    public async Task<ReportsSummaryDto> GetSummaryAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        // Project only scalar columns — avoids JSON value converters for
        // image[] and highlights_json which can contain malformed data.
        var eventsQ = _db.Events
            .AsNoTracking()
            .Select(e => new
            {
                e.Id,
                e.Status,
                e.Category,
                e.DateAd,
                e.Price,
                e.Rating,
                e.ReviewsLabel,
                e.Title,
                e.Location,
                e.EntryType,
                e.RequiresInvitation,
                e.RequiresRegistration
            });

        if (from.HasValue) eventsQ = eventsQ.Where(e => e.DateAd >= from.Value);
        if (to.HasValue)   eventsQ = eventsQ.Where(e => e.DateAd <= to.Value);

        var allEvents = await eventsQ.ToListAsync(cancellationToken);

        // ── Event counts ──────────────────────────────────────────────────────
        var totalEvents           = allEvents.Count;
        var publishedEvents       = allEvents.Count(e => e.Status == EventLifecycleStatus.Published);
        var draftEvents           = allEvents.Count(e => e.Status == EventLifecycleStatus.Draft);
        var pendingApprovalEvents = allEvents.Count(e => e.Status == EventLifecycleStatus.PendingApproval);
        var archivedEvents        = allEvents.Count(e => e.Status == EventLifecycleStatus.Archived);
        var freeEvents            = allEvents.Count(e => e.Price == 0);
        var paidEvents            = allEvents.Count(e => e.Price > 0);

        // ── Users ─────────────────────────────────────────────────────────────
        var allUsers = await _db.Users
            .AsNoTracking()
            .Select(u => new { u.IsActive, u.Role })
            .ToListAsync(cancellationToken);

        var totalUsers  = allUsers.Count;
        var activeUsers = allUsers.Count(u => u.IsActive);
        var adminUsers  = allUsers.Count(u => u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin);
        var clientUsers = allUsers.Count(u => u.Role == UserRole.Client);

        // ── Invitations ───────────────────────────────────────────────────────
        var invQ = _db.Invitations
            .AsNoTracking()
            .Select(i => new { i.Id, i.EventId, i.Status });

        if (from.HasValue || to.HasValue)
        {
            var eventIds = allEvents.Select(e => e.Id).ToHashSet();
            invQ = invQ.Where(i => eventIds.Contains(i.EventId));
        }

        var allInvitations = await invQ.ToListAsync(cancellationToken);
        var totalInvitations     = allInvitations.Count;
        var pendingInvitations   = allInvitations.Count(i => i.Status == InvitationStatus.Pending);
        var sentInvitations      = allInvitations.Count(i => i.Status == InvitationStatus.Sent);
        var verifiedInvitations  = allInvitations.Count(i => i.Status == InvitationStatus.Verified);
        var expiredInvitations   = allInvitations.Count(i => i.Status == InvitationStatus.Expired);
        var cancelledInvitations = allInvitations.Count(i => i.Status == InvitationStatus.Cancelled);

        // ── Scans ─────────────────────────────────────────────────────────────
        var invitationIds = allInvitations.Select(i => i.Id).ToHashSet();
        var allScans = await _db.InvitationScans
            .AsNoTracking()
            .Where(s => invitationIds.Contains(s.InvitationId))
            .Select(s => new { s.InvitationId, s.Result })
            .ToListAsync(cancellationToken);

        var totalScans         = allScans.Count;
        var successfulCheckIns = allScans.Count(s => s.Result == ScanResult.Verified);

        // ── Registrations ─────────────────────────────────────────────────────
        var regQ = _db.EventRegistrations
            .AsNoTracking()
            .Select(r => new { r.Id, r.EventId, r.Status });

        if (from.HasValue || to.HasValue)
        {
            var eventIds = allEvents.Select(e => e.Id).ToHashSet();
            regQ = regQ.Where(r => eventIds.Contains(r.EventId));
        }

        var allRegistrations = await regQ.ToListAsync(cancellationToken);

        // ── Workshop invites (bulk, plain-text — a separate mechanism admins can
        //    use on any event alongside or instead of Invitations/Registrations) ──
        var workshopQ = _db.WorkshopInvites
            .AsNoTracking()
            .Select(w => new { w.Id, w.EventId, w.Status });

        if (from.HasValue || to.HasValue)
        {
            var eventIds = allEvents.Select(e => e.Id).ToHashSet();
            workshopQ = workshopQ.Where(w => eventIds.Contains(w.EventId));
        }

        var allWorkshopInvites = await workshopQ.ToListAsync(cancellationToken);

        // ── Events by category ────────────────────────────────────────────────
        var categoryColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Festival"]       = "#f8ce1c",
            ["Adventure"]      = "#1c5c6d",
            ["Cultural"]       = "#3f515b",
            ["Spiritual"]      = "#7c3aed",
            ["Food & Cuisine"] = "#bd242b",
            ["Meeting"]        = "#0369a1",
        };

        var eventsByCategory = allEvents
            .GroupBy(e => e.Category)
            .Select(g => new CategoryBreakdown(
                g.Key,
                categoryColors.TryGetValue(g.Key, out var c) ? c : "#64748b",
                g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        // ── Events by month ───────────────────────────────────────────────────
        var eventsByMonth = allEvents
            .GroupBy(e => new { e.DateAd.Year, e.DateAd.Month })
            .Select(g => new MonthlyBreakdown(
                g.Key.Year,
                g.Key.Month,
                new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                g.Count()))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        // ── Top events by rating ──────────────────────────────────────────────
        var topEvents = allEvents
            .Where(e => e.Status == EventLifecycleStatus.Published)
            .OrderByDescending(e => e.Rating)
            .Take(5)
            .Select(e => new TopEventDto(
                e.Id,
                e.Title,
                e.Category,
                e.Location,
                e.Rating,
                e.ReviewsLabel,
                e.EntryType,
                e.Price))
            .ToList();

        // ── Per-event invitation & registration report ────────────────────────
        var invitationsByEvent  = allInvitations.ToLookup(i => i.EventId);
        var registrationsByEvent = allRegistrations.ToLookup(r => r.EventId);
        var workshopInvitesByEvent = allWorkshopInvites.ToLookup(w => w.EventId);
        var eventIdToInvitationIds = allInvitations
            .GroupBy(i => i.EventId)
            .ToDictionary(g => g.Key, g => g.Select(i => i.Id).ToHashSet());

        var eventsReport = allEvents
            .OrderByDescending(e => e.DateAd)
            .Select(e =>
            {
                var invitations = invitationsByEvent[e.Id].ToList();
                var registrations = registrationsByEvent[e.Id].ToList();
                var workshopInvites = workshopInvitesByEvent[e.Id].ToList();
                var eventInvitationIds = eventIdToInvitationIds.TryGetValue(e.Id, out var ids) ? ids : new HashSet<long>();
                var eventCheckIns = allScans.Count(s =>
                    s.Result == ScanResult.Verified && eventInvitationIds.Contains(s.InvitationId));

                return new EventReportRow(
                    e.Id,
                    e.Title,
                    e.Category,
                    e.Status.ToString(),
                    e.DateAd,
                    e.RequiresInvitation,
                    e.RequiresRegistration,

                    invitations.Count,
                    invitations.Count(i => i.Status == InvitationStatus.Pending),
                    invitations.Count(i => i.Status == InvitationStatus.Sent),
                    invitations.Count(i => i.Status == InvitationStatus.Verified),
                    invitations.Count(i => i.Status == InvitationStatus.Expired),
                    invitations.Count(i => i.Status == InvitationStatus.Cancelled),

                    registrations.Count,
                    registrations.Count(r => r.Status == RegistrationStatus.Pending),
                    registrations.Count(r => r.Status == RegistrationStatus.Approved),
                    registrations.Count(r => r.Status == RegistrationStatus.Rejected),
                    registrations.Count(r => r.Status == RegistrationStatus.Cancelled),

                    eventCheckIns,

                    workshopInvites.Count,
                    workshopInvites.Count(w => w.Status == WorkshopInviteStatus.Pending),
                    workshopInvites.Count(w => w.Status == WorkshopInviteStatus.Sent),
                    workshopInvites.Count(w => w.Status == WorkshopInviteStatus.Verified));
            })
            .ToList();

        return new ReportsSummaryDto(
            totalEvents, publishedEvents, draftEvents, pendingApprovalEvents, archivedEvents, freeEvents, paidEvents,
            totalUsers, activeUsers, adminUsers, clientUsers,
            totalInvitations, pendingInvitations, sentInvitations, verifiedInvitations, expiredInvitations, cancelledInvitations,
            totalScans, successfulCheckIns,
            eventsByCategory, eventsByMonth, topEvents, eventsReport);
    }
}

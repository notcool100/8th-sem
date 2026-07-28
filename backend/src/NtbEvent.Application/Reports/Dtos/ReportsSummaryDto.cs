namespace NtbEvent.Application.Reports.Dtos;

public sealed record ReportsSummaryDto(
    // ── Events ────────────────────────────────────────────────────────────────
    int TotalEvents,
    int PublishedEvents,
    int DraftEvents,
    int PendingApprovalEvents,
    int ArchivedEvents,
    int FreeEvents,
    int PaidEvents,

    // ── Users ─────────────────────────────────────────────────────────────────
    int TotalUsers,
    int ActiveUsers,
    int AdminUsers,
    int ClientUsers,

    // ── Invitations ───────────────────────────────────────────────────────────
    int TotalInvitations,
    int PendingInvitations,
    int SentInvitations,
    int VerifiedInvitations,
    int ExpiredInvitations,
    int CancelledInvitations,

    // ── Check-ins ─────────────────────────────────────────────────────────────
    int TotalScans,
    int SuccessfulCheckIns,

    // ── Breakdowns ────────────────────────────────────────────────────────────
    IReadOnlyList<CategoryBreakdown> EventsByCategory,
    IReadOnlyList<MonthlyBreakdown> EventsByMonth,
    IReadOnlyList<TopEventDto> TopEvents,
    IReadOnlyList<EventReportRow> EventsReport
);

public sealed record CategoryBreakdown(string Category, string Color, int Count);

public sealed record MonthlyBreakdown(int Year, int Month, string Label, int Count);

public sealed record TopEventDto(
    long Id,
    string Title,
    string Category,
    string Location,
    decimal Rating,
    string ReviewsLabel,
    string EntryType,
    decimal Price
);

/// <summary>Per-event invitation and registration figures shown in the Reports events table.</summary>
public sealed record EventReportRow(
    long Id,
    string Title,
    string Category,
    string Status,
    DateTime DateAd,
    bool RequiresInvitation,
    bool RequiresRegistration,

    int TotalInvitations,
    int PendingInvitations,
    int SentInvitations,
    int VerifiedInvitations,
    int ExpiredInvitations,
    int CancelledInvitations,

    int TotalRegistrations,
    int PendingRegistrations,
    int ApprovedRegistrations,
    int RejectedRegistrations,
    int CancelledRegistrations,

    int SuccessfulCheckIns,

    // ── Bulk workshop invites (separate from the QR Invitations flow) ────────
    int TotalWorkshopInvites,
    int PendingWorkshopInvites,
    int SentWorkshopInvites,
    int VerifiedWorkshopInvites
);

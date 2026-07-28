using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

public sealed class Event : BaseEntity
{

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string LongDescription { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public EventType Type { get; set; } = EventType.Event;

    public EventLifecycleStatus Status { get; set; } = EventLifecycleStatus.Draft;

    public DateTime DateAd { get; set; }

    public DateTime EndDateAd { get; set; }

    public string DateBs { get; set; } = string.Empty;

    public string EndDateBs { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string DateRangeLabel { get; set; } = string.Empty;

    public string DurationLabel { get; set; } = string.Empty;

    public string AttendanceLabel { get; set; } = string.Empty;

    public string AttendanceNote { get; set; } = string.Empty;

    public string EntryType { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Rating { get; set; }

    public string ReviewsLabel { get; set; } = string.Empty;

    // public List<string> Tags { get; set; } = [];

    public List<string> Image { get; set; } = [];

    public string MapImage { get; set; } = string.Empty;

    public string Organizer { get; set; } = string.Empty;

    public string OrganizerSubtitle { get; set; } = string.Empty;

    public bool OrganizerVerified { get; set; }

    public string? OrganizerImageUrl { get; set; }

    public List<EventHighlight> Highlights { get; set; } = [];

    public bool Featured { get; set; }

    public string ReadTime { get; set; } = string.Empty;

    /// <summary>When true, normal users can self-register for this event via the public registration link.</summary>
    public bool RequiresRegistration { get; set; }

    /// <summary>When true, only admin-issued invitations grant entry; self-registration is disabled.</summary>
    public bool RequiresInvitation { get; set; }

    /// <summary>When true, workshop invite emails omit the invite link and the public invite page is unreachable; entry is QR-scan-only.</summary>
    public bool QrOnlyCheckIn { get; set; }

    /// <summary>When true, the entry type badge (Free/Paid) is shown on public event pages. False hides it.</summary>
    public bool ShowEntryType { get; set; } = true;

    /// <summary>Optional per-event override for the invitation email subject. Falls back to the global "Event Invitation" template when null/blank.</summary>
    public string? InvitationEmailSubject { get; set; }

    /// <summary>Optional per-event override for the invitation email body (HTML). Falls back to the global "Event Invitation" template when null/blank.</summary>
    public string? InvitationEmailBodyHtml { get; set; }

    /// <summary>Keys of the optional self-registration fields the admin has enabled for this event's public registration form. `fullName`/`email` are always collected regardless of this list.</summary>
    public List<string> SelfRegistrationFields { get; set; } = [];

    public long? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public long? UpdatedById { get; set; }

    public User? UpdatedBy { get; set; }

    public ICollection<EventApprovalRequest> ApprovalRequests { get; set; } = [];
}

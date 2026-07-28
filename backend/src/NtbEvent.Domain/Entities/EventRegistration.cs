using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

/// <summary>
/// A guest's self-submitted request to attend an event that has
/// <see cref="Event.RequiresRegistration"/> set. Guest details are normalized
/// away into <see cref="Guest"/>, same as <see cref="Invitation"/>. A unique
/// (EventId, GuestId) pair prevents duplicate registrations of the same person
/// for the same event.
/// </summary>
public sealed class EventRegistration : BaseEntity
{
    public long EventId { get; set; }

    public Event Event { get; set; } = default!;

    public long GuestId { get; set; }

    public Guest Guest { get; set; } = default!;

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

    public DateTime RequestedAtUtc { get; set; }

    /// <summary>Admin who approved/rejected the registration.</summary>
    public long? ReviewedByUserId { get; set; }

    public User? ReviewedBy { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    /// <summary>Submitted values for the event's enabled optional self-registration fields, keyed by field key.</summary>
    public Dictionary<string, string> AdditionalFields { get; set; } = new();
}

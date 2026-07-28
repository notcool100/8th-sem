using NtbEvent.Domain.Common;

namespace NtbEvent.Domain.Entities;

/// <summary>
/// A person who can be invited to events. Stored once per unique (normalized)
/// email so the same individual is reused across many invitations — this keeps
/// the schema normalized (guest attributes live here, not on each invitation).
/// </summary>
public sealed class Guest : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public ICollection<Invitation> Invitations { get; set; } = [];

    public ICollection<EventRegistration> Registrations { get; set; } = [];
}

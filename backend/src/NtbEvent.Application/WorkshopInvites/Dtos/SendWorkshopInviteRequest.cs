using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.WorkshopInvites.Dtos;

public sealed class SendWorkshopInviteRequest
{
    /// <summary>The date this guest should attend, as an ISO date (yyyy-MM-dd). May differ from the event's start date for multi-day workshops.</summary>
    [Required, MaxLength(20)]
    public string WorkshopDate { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.WorkshopInvites.Dtos;

public sealed class ScanWorkshopInviteRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

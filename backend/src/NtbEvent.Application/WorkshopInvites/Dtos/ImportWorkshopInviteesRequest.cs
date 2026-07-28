using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.WorkshopInvites.Dtos;

public sealed class ImportWorkshopInviteeRow
{
    [Required, MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Phone { get; set; }

    [MaxLength(160)]
    public string? Organization { get; set; }
}

public sealed class ImportWorkshopInviteesRequest
{
    [Required]
    public List<ImportWorkshopInviteeRow> Rows { get; set; } = [];
}

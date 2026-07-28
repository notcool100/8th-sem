namespace NtbEvent.Application.WorkshopInvites.Dtos;

public sealed class WorkshopInviteScanResultDto
{
    /// <summary>"previewed", "alreadyused" or "invalid".</summary>
    public string Result { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool CanVerify { get; set; }

    public WorkshopInviteDto? Invite { get; set; }
}

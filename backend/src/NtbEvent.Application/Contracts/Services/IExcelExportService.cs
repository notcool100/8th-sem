using NtbEvent.Application.Invitations.Dtos;
using NtbEvent.Application.Registrations.Dtos;
using NtbEvent.Application.WorkshopInvites.Dtos;

namespace NtbEvent.Application.Contracts.Services;

/// <summary>Builds .xlsx workbooks for the admin "download attendee list" actions.</summary>
public interface IExcelExportService
{
    byte[] BuildRegistrationsWorkbook(IReadOnlyList<EventRegistrationDto> registrations);

    byte[] BuildInvitationsWorkbook(IReadOnlyList<InvitationDto> invitations);

    byte[] BuildWorkshopInvitesWorkbook(IReadOnlyList<WorkshopInviteDto> invites);
}

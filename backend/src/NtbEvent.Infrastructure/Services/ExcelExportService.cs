using System.Globalization;
using ClosedXML.Excel;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Invitations.Dtos;
using NtbEvent.Application.Registrations.Dtos;
using NtbEvent.Application.WorkshopInvites.Dtos;

namespace NtbEvent.Infrastructure.Services;

/// <summary>Builds .xlsx workbooks using ClosedXML (fully-managed, no Excel/native deps).</summary>
public sealed class ExcelExportService : IExcelExportService
{
    private const string DateFormat = "yyyy-mm-dd hh:mm";

    public byte[] BuildRegistrationsWorkbook(IReadOnlyList<EventRegistrationDto> registrations)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Registrations");

        var extraKeys = new List<string>();
        foreach (var registration in registrations)
        {
            foreach (var key in registration.AdditionalFields.Keys)
            {
                if (!extraKeys.Contains(key))
                {
                    extraKeys.Add(key);
                }
            }
        }

        var headers = new List<string> { "Name", "Email", "Phone", "Organization", "Status", "Requested At (UTC)", "Reviewed At (UTC)" };
        headers.AddRange(extraKeys.Select(Humanize));
        WriteHeader(sheet, headers);

        var row = 2;
        foreach (var registration in registrations)
        {
            var col = 1;
            sheet.Cell(row, col++).Value = registration.GuestName;
            sheet.Cell(row, col++).Value = registration.GuestEmail;
            sheet.Cell(row, col++).Value = registration.GuestPhone;
            sheet.Cell(row, col++).Value = registration.GuestOrganization;
            sheet.Cell(row, col++).Value = registration.Status;
            WriteDate(sheet, row, col++, registration.RequestedAtUtc);
            WriteDate(sheet, row, col++, registration.ReviewedAtUtc);
            foreach (var key in extraKeys)
            {
                sheet.Cell(row, col++).Value = registration.AdditionalFields.GetValueOrDefault(key, string.Empty);
            }
            row++;
        }

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    public byte[] BuildInvitationsWorkbook(IReadOnlyList<InvitationDto> invitations)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Invitations");
        WriteHeader(sheet, ["Name", "Email", "Phone", "Organization", "Status", "Sent At (UTC)", "Expires At (UTC)", "Checked In At (UTC)"]);

        var row = 2;
        foreach (var invitation in invitations)
        {
            sheet.Cell(row, 1).Value = invitation.GuestName;
            sheet.Cell(row, 2).Value = invitation.GuestEmail;
            sheet.Cell(row, 3).Value = invitation.GuestPhone;
            sheet.Cell(row, 4).Value = invitation.GuestOrganization;
            sheet.Cell(row, 5).Value = invitation.Status;
            WriteDate(sheet, row, 6, invitation.SentAtUtc);
            WriteDate(sheet, row, 7, invitation.ExpiresAtUtc);
            WriteDate(sheet, row, 8, invitation.VerifiedAtUtc);
            row++;
        }

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    public byte[] BuildWorkshopInvitesWorkbook(IReadOnlyList<WorkshopInviteDto> invites)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Workshop Invites");
        WriteHeader(sheet, ["Name", "Email", "Phone", "Organization", "Status", "Sent At (UTC)", "Checked In At (UTC)"]);

        var row = 2;
        foreach (var invite in invites)
        {
            sheet.Cell(row, 1).Value = invite.FullName;
            sheet.Cell(row, 2).Value = invite.Email;
            sheet.Cell(row, 3).Value = invite.Phone;
            sheet.Cell(row, 4).Value = invite.Organization;
            sheet.Cell(row, 5).Value = invite.Status;
            WriteDate(sheet, row, 6, invite.SentAtUtc);
            WriteDate(sheet, row, 7, invite.VerifiedAtUtc);
            row++;
        }

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    private static void WriteHeader(IXLWorksheet sheet, IReadOnlyList<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1C5C6D");
        }
        sheet.SheetView.FreezeRows(1);
    }

    private static void WriteDate(IXLWorksheet sheet, int row, int col, DateTime? value)
    {
        if (value is not { } date)
        {
            return;
        }
        var cell = sheet.Cell(row, col);
        cell.Value = date;
        cell.Style.DateFormat.Format = DateFormat;
    }

    private static string Humanize(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return key;
        }
        var spaced = key.Replace('_', ' ').Replace('-', ' ');
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(spaced);
    }

    private static byte[] ToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

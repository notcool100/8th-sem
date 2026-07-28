namespace NtbEvent.Application.Contracts.Services;

public interface IQrCodeService
{
    /// <summary>Renders <paramref name="payload"/> as a PNG QR code.</summary>
    byte[] GeneratePng(string payload, int pixelsPerModule = 10);

    /// <summary>Renders <paramref name="payload"/> as a data URI (image/png;base64).</summary>
    string GenerateDataUri(string payload, int pixelsPerModule = 10);
}

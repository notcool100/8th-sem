using NtbEvent.Application.Contracts.Services;
using QRCoder;

namespace NtbEvent.Infrastructure.Services;

/// <summary>
/// Generates QR codes using QRCoder's fully-managed PNG renderer
/// (<see cref="PngByteQRCode"/>) — no System.Drawing / native deps, so it runs
/// the same on Linux, macOS and Windows.
/// </summary>
public sealed class QrCodeService : IQrCodeService
{
    public byte[] GeneratePng(string payload, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }

    public string GenerateDataUri(string payload, int pixelsPerModule = 10)
    {
        var png = GeneratePng(payload, pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }
}

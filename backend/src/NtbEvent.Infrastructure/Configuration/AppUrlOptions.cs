namespace NtbEvent.Infrastructure.Configuration;

/// <summary>
/// Public-facing URLs used when generating links emailed to guests.
/// </summary>
public sealed class AppUrlOptions
{
    public const string SectionName = "AppUrls";

    /// <summary>Base URL of the frontend, e.g. http://localhost:5173.</summary>
    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";
}

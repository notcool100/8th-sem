namespace NtbEvent.Application.Contracts.Services;

/// <summary>
/// Supplies public-facing URLs (configured per environment) used when building
/// invitation links that are emailed to guests.
/// </summary>
public interface IAppUrlProvider
{
    /// <summary>Builds the absolute guest-facing invite URL for a token.</summary>
    string BuildInviteUrl(string token);

    /// <summary>Builds the absolute guest-facing workshop-invite URL for a token.</summary>
    string BuildWorkshopInviteUrl(string token);
}

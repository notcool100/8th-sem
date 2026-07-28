using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Infrastructure.Configuration;

namespace NtbEvent.Infrastructure.Services;

public sealed class AppUrlProvider : IAppUrlProvider
{
    private readonly AppUrlOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public AppUrlProvider(
        IOptions<AppUrlOptions> options,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public string BuildInviteUrl(string token)
    {
        var baseUrl = ResolveFrontendBaseUrl();
        return $"{baseUrl}/invite/{token}";
    }

    public string BuildWorkshopInviteUrl(string token)
    {
        var baseUrl = ResolveFrontendBaseUrl();
        return $"{baseUrl}/workshop-invite/{token}";
    }

    // Derives the frontend origin from the request that triggered the call (e.g. the
    // admin sending an invite from the SvelteKit app) instead of a hardcoded config
    // value, so links work on whatever domain the app is actually deployed to.
    // Only origins already trusted via Cors:AllowedOrigins are used, to stop a
    // spoofed Origin header from redirecting invite links to an attacker's domain.
    private string ResolveFrontendBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var origin = request?.Headers.Origin.ToString();

        if (string.IsNullOrWhiteSpace(origin) && request is not null)
        {
            var referer = request.Headers.Referer.ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            {
                origin = $"{refererUri.Scheme}://{refererUri.Authority}";
            }
        }

        if (!string.IsNullOrWhiteSpace(origin))
        {
            var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                return origin.TrimEnd('/');
            }
        }

        return (_options.FrontendBaseUrl ?? string.Empty).TrimEnd('/');
    }
}

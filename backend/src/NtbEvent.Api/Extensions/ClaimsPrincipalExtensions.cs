using System.Security.Claims;

namespace NtbEvent.Api.Extensions;

internal static class ClaimsPrincipalExtensions
{
    public static long? GetUserId(this ClaimsPrincipal principal)
    {
        var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        return long.TryParse(claimValue, out var userId) ? userId : null;
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NtbEvent.Infrastructure.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirstValue("role")
                   ?? Context.User?.FindFirstValue(ClaimTypes.Role)
                   ?? string.Empty;

        if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        await base.OnConnectedAsync();
    }
}

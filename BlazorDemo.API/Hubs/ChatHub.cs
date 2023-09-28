using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorDemo.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        var u = Context.User;
        await Clients.All.SendAsync("ReceiveMessage",
            user,
            message,
            DateTime.Now.ToShortTimeString()
        );
    }
}
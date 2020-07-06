using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CareerExpansionMod.Hubs
{
    public class InfoHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            if (Clients != null)
            {
                await Clients.All.SendAsync("ReceiveMessage", user, message);
            }
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}

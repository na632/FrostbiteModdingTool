using NetDiscordRpc;
using NetDiscordRpc.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace FrostbiteSdk
{
    [SupportedOSPlatform("windows")]
    public class DiscordInterop
    {
        public static DiscordRPC DiscordRpcClient;

        public async Task<DiscordRPC> StartDiscordClient()
        {
            return await Task.Run(() =>
            {
                if (DiscordRpcClient == null)
                {
                    DiscordRpcClient = new DiscordRPC("836520037208686652");
                    DiscordRpcClient.Logger = new NetDiscordRpc.Core.Logger.NullLogger();
                    DiscordRpcClient.Initialize();
                    DiscordRpcClient.SetPresence(new RichPresence()
                    {
                        Details = "In Main Menu",
                        State = "In Main Menu",
                    });
                    DiscordRpcClient.Invoke();

                }
                return DiscordRpcClient;
            });
        }

        public DiscordRPC GetDiscordRPC()
        {
            return StartDiscordClient().Result;
        }
    }
}

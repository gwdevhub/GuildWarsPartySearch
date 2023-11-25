using GuildWarsPartySearch.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MTSC;
using MTSC.ServerSide;
using MTSC.ServerSide.Handlers;

namespace GuildWarsPartySearch.Server.ServerHandlers;

public sealed class StartupHandler : IHandler
{
    private bool initialized;

    public void ClientRemoved(MTSC.ServerSide.Server server, ClientData client) { }

    public bool HandleClient(MTSC.ServerSide.Server server, ClientData client) => false;

    public bool HandleReceivedMessage(MTSC.ServerSide.Server server, ClientData client, Message message) => false;

    public bool HandleSendMessage(MTSC.ServerSide.Server server, ClientData client, ref Message message) => false;

    public bool PreHandleReceivedMessage(MTSC.ServerSide.Server server, ClientData client, ref Message message) => false;

    public void Tick(MTSC.ServerSide.Server server)
    {
        if (!this.initialized)
        {
            this.initialized = true;
            var options = server.ServiceManager.GetRequiredService<IOptions<EnvironmentOptions>>();
            server.Log($"Running environment {options.Value.Name}");
        }
    }
}

using MTSC.Common.WebSockets;
using Newtonsoft.Json;
using System.Core.Extensions;
using System.Net.WebSockets;
using System.Text;

namespace GuildWarsPartySearch.Server.Services.Feed;

public sealed class LiveFeedService : ILiveFeedService
{
    private static readonly List<WebSocket> Clients = [];

    private readonly ILogger<LiveFeedService> logger;

    public LiveFeedService(
        ILogger<LiveFeedService> logger)
    {
        this.logger = logger.ThrowIfNull();
    }

    public void AddClient(WebSocket client)
    {
        AddClientInternal(client);
    }

    public async Task PushUpdate(Models.PartySearch partySearchUpdate, CancellationToken cancellationToken)
    {
        var payloadString = JsonConvert.SerializeObject(partySearchUpdate);
        var payload = Encoding.UTF8.GetBytes(payloadString);
        var websocketMessage = new WebsocketMessage
        {
            Data = payload,
            FIN = true,
            Opcode = WebsocketMessage.Opcodes.Text
        };
        var messageBytes = websocketMessage.GetMessageBytes();
        await ExecuteOnClientsInternal(async client =>
        {
            try
            {
                await client.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, $"Encountered exception while broadcasting update");
            }
        });
    }

    public void RemoveClient(WebSocket client)
    {
        RemoveClientInternal(client);
    }

    private static void AddClientInternal(WebSocket client)
    {
        while (!Monitor.TryEnter(Clients)) { }
        
        Clients.Add(client);
        
        Monitor.Exit(Clients);
    }

    private static void RemoveClientInternal(WebSocket client)
    {
        while (!Monitor.TryEnter(Clients)) { }

        Clients.Remove(client);

        Monitor.Exit(Clients);
    }

    private static async Task ExecuteOnClientsInternal(Func<WebSocket, Task> action)
    {
        while (!Monitor.TryEnter(Clients)) { }

        foreach (var client in Clients)
        {
            await action(client);
        }

        Monitor.Exit(Clients);
    }
}

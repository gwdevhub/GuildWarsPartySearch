using GuildWarsPartySearch.Server.Models.Endpoints;
using Microsoft.AspNetCore.Mvc;
using System.Core.Extensions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace GuildWarsPartySearch.Server.Services.Feed;

public sealed class LiveFeedService : ILiveFeedService
{
    private static readonly List<WebSocket> Clients = [];

    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly ILogger<LiveFeedService> logger;

    public LiveFeedService(
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<LiveFeedService> logger)
    {
        this.jsonSerializerOptions = jsonSerializerOptions.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public void AddClient(WebSocket client)
    {
        AddClientInternal(client);
    }

    public async Task PushUpdate(Models.PartySearch partySearchUpdate, CancellationToken cancellationToken)
    {
        // Since LiveFeed endpoint expects a PartySearchList, so we send a PartySearchList with only the update to keep it consistent
        var payloadString = JsonSerializer.Serialize(new PartySearchList { Searches = [partySearchUpdate] }, this.jsonSerializerOptions);
        var payload = Encoding.UTF8.GetBytes(payloadString);
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

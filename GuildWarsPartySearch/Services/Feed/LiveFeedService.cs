using GuildWarsPartySearch.Server.Models.Endpoints;
using System.Core.Extensions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace GuildWarsPartySearch.Server.Services.Feed;

public sealed class LiveFeedService : ILiveFeedService
{
    private readonly SemaphoreSlim semaphore = new(1);
    private readonly List<WebSocket> clients = [];
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly ILogger<LiveFeedService> logger;

    public LiveFeedService(
        IHostApplicationLifetime lifetime,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<LiveFeedService> logger)
    {
        lifetime.ApplicationStopping.Register(this.ShutDownConnections);
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

    private void AddClientInternal(WebSocket client)
    {
        this.semaphore.Wait();
        this.clients.Add(client);
        this.semaphore.Release();
    }

    private void RemoveClientInternal(WebSocket client)
    {
        this.semaphore.Wait();
        this.clients.Remove(client);
        this.semaphore.Release();
    }

    private async Task ExecuteOnClientsInternal(Func<WebSocket, Task> action)
    {
        await this.semaphore.WaitAsync();
        await Task.WhenAll(this.clients.Select(client => action(client)));
        this.semaphore.Release();
    }

    private void ShutDownConnections()
    {
        this.semaphore.Wait();
        foreach(var client in this.clients)
        {
            client.Abort();
        }

        this.semaphore.Release();
    }
}

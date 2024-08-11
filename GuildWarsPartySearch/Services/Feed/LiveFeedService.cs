using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.Database;
using System.Core.Extensions;
using System.Extensions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace GuildWarsPartySearch.Server.Services.Feed;

public sealed class LiveFeedService : ILiveFeedService
{
    private const int MaxConnectionsPerIP = 2;

    private readonly SemaphoreSlim semaphore = new(1);
    private readonly Dictionary<string, List<WebSocket>> clients = [];
    private readonly IIpWhitelistDatabase ipWhitelistDatabase;
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly ILogger<LiveFeedService> logger;

    public LiveFeedService(
        IHostApplicationLifetime lifetime,
        IIpWhitelistDatabase ipWhitelistDatabase,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<LiveFeedService> logger)
    {
        lifetime.ApplicationStopping.Register(this.ShutDownConnections);
        this.ipWhitelistDatabase = ipWhitelistDatabase.ThrowIfNull();
        this.jsonSerializerOptions = jsonSerializerOptions.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public Task<bool> AddClient(WebSocket client, string? ipAddress, CancellationToken cancellationToken)
    {
        return AddClientInternal(client, ipAddress, cancellationToken);
    }

    public async Task PushUpdate(Models.PartySearch partySearchUpdate, CancellationToken cancellationToken)
    {
        // Since LiveFeed endpoint expects a PartySearchList, so we send a PartySearchList with only the update to keep it consistent
        var payloadString = JsonSerializer.Serialize(new PartySearchList { Searches = [partySearchUpdate] }, this.jsonSerializerOptions);
        var payload = Encoding.UTF8.GetBytes(payloadString);
        await ExecuteOnClientsInternal(async (address, client) =>
        {
            try
            {
                await client.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, $"Encountered exception while broadcasting update");
                RemoveClientInternal(client, address);
            }
        });
    }

    public void RemoveClient(WebSocket client, string? ipAddress)
    {
        RemoveClientInternal(client, ipAddress);
    }

    private async Task<bool> AddClientInternal(WebSocket client, string? ipAddress, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.AddClientInternal), ipAddress ?? string.Empty);

        await this.semaphore.WaitAsync(cancellationToken);
        try
        {
            if (ipAddress is null ||
                ipAddress.IsNullOrWhiteSpace())
            {
                return false;
            }

            var whitelistedIps = await this.ipWhitelistDatabase.GetWhitelistedAddresses(cancellationToken);
            if (whitelistedIps.None(addr => addr == ipAddress) &&
                this.clients.TryGetValue(ipAddress, out var sockets) &&
                sockets.Count >= 2)
            {
                scopedLogger.LogError("Too many live connections. Rejecting");
                return false;
            }

            if (!this.clients.TryGetValue(ipAddress, out var existingSockets))
            {
                existingSockets = [];
                this.clients[ipAddress] = existingSockets;
            }

            existingSockets.Add(client);
            return true;
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    private void RemoveClientInternal(WebSocket client, string? ipAddress)
    {
        this.semaphore.Wait();
        if (ipAddress is null ||
            ipAddress.IsNullOrWhiteSpace())
        {
            return;
        }

        if (this.clients.TryGetValue(ipAddress, out var sockets))
        {
            sockets.Remove(client);
            if (sockets.Count == 0)
            {
                this.clients.Remove(ipAddress);
            }
        }

        if (client?.State is not WebSocketState.Closed or WebSocketState.Aborted)
        {
            client?.Abort();
        }

        this.semaphore.Release();
    }

    private async Task ExecuteOnClientsInternal(Func<string, WebSocket, Task> action)
    {
        await this.semaphore.WaitAsync();
        await Task.WhenAll(this.clients.SelectMany(pair => pair.Value.Select(client => action(pair.Key, client))));
        this.semaphore.Release();
    }

    private void ShutDownConnections()
    {
        this.semaphore.Wait();
        foreach(var sockets in this.clients.Values)
        {
            foreach(var client in sockets)
            {
                client.Abort();
            }
        }

        this.semaphore.Release();
    }
}

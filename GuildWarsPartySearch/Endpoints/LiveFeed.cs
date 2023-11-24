using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.Extensions.Logging;
using MTSC.Common.WebSockets;
using MTSC.Common.WebSockets.RoutingModules;
using Newtonsoft.Json;
using System.Core.Extensions;
using System.Extensions;
using System.Text;

namespace GuildWarsPartySearch.Server.Endpoints;

public sealed class LiveFeed : WebsocketRouteBase<None>
{
    private readonly IPartySearchService partySearchService;
    private readonly ILiveFeedService liveFeedService;
    private readonly ILogger<LiveFeed> logger;

    public LiveFeed(
        IPartySearchService partySearchService,
        ILiveFeedService liveFeedService,
        ILogger<LiveFeed> logger)
    {
        this.partySearchService = partySearchService.ThrowIfNull();
        this.liveFeedService = liveFeedService.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public override void ConnectionClosed()
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.ConnectionInitialized), this.ClientData.Socket.RemoteEndPoint?.ToString() ?? string.Empty);
        scopedLogger.LogInformation("Client connected");
        this.liveFeedService.RemoveClient(this.ClientData);
    }

    public override async void ConnectionInitialized()
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.ConnectionInitialized), this.ClientData.Socket.RemoteEndPoint?.ToString() ?? string.Empty);
        scopedLogger.LogInformation("Client connected");
        this.liveFeedService.AddClient(this.ClientData);
        scopedLogger.LogInformation("Sending all party searches");
        var updates = await this.partySearchService.GetAllPartySearches();
        var serialized = JsonConvert.SerializeObject(updates);
        var payload = Encoding.UTF8.GetBytes(serialized);
        this.SendMessage(new WebsocketMessage
        {
            Data = payload,
            FIN = true,
            Opcode = WebsocketMessage.Opcodes.Text
        });
    }

    public override void HandleReceivedMessage(None message)
    {
    }

    public override void Tick()
    {
    }
}

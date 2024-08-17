using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.BotStatus;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.PartySearch;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

public sealed class LiveFeed : WebSocketRouteBase<LiveFeedRequest, PartySearchList>
{
    private readonly IBotStatusService botStatusService;
    private readonly IPartySearchService partySearchService;
    private readonly ILiveFeedService liveFeedService;
    private readonly ILogger<LiveFeed> logger;

    public LiveFeed(
        IBotStatusService botStatusService,
        IPartySearchService partySearchService,
        ILiveFeedService liveFeedService,
        ILogger<LiveFeed> logger)
    {
        this.botStatusService = botStatusService.ThrowIfNull();
        this.partySearchService = partySearchService.ThrowIfNull();
        this.liveFeedService = liveFeedService.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public override async Task ExecuteAsync(LiveFeedRequest? message, CancellationToken cancellationToken)
    {
        if (message?.GetFullList is not true)
        {
            return;
        }

        var bots = await this.botStatusService.GetBots(cancellationToken);
        var searches = await this.partySearchService.GetAllPartySearches(cancellationToken);
        await this.SendMessage(new PartySearchList
        {
            Searches = searches.Where(u => bots.Any(b => b.Map == u.Map && b.District == u.District)).ToList() // Filter searches by current active locations
        }, cancellationToken);
    }

    public override async Task SocketAccepted(CancellationToken cancellationToken)
    {
        var ipAddress = this.Context?.Request.HttpContext.GetClientIP();
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketAccepted), string.Empty);
        if (!await this.liveFeedService.AddClient(this.WebSocket!, ipAddress, this.Context?.GetPermissionLevel() ?? Models.PermissionLevel.None, cancellationToken))
        {
            scopedLogger.LogError("Client rejected");
            this.WebSocket?.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Connection rejected", cancellationToken);
            return;
        }

        scopedLogger.LogDebug("Client accepted to livefeed");
        scopedLogger.LogDebug("Sending all party searches");
        var updates = await this.partySearchService.GetAllPartySearches(cancellationToken);
        var bots = await this.botStatusService.GetBots(cancellationToken);
        await this.SendMessage(new PartySearchList
        { 
            Searches = updates.Where(u => bots.Any(b => b.Map == u.Map && b.District == u.District)).ToList() // Filter searches by current active locations
        }, cancellationToken);
    }

    public override Task SocketClosed()
    {
        var ipAddress = this.Context?.Request.HttpContext.GetClientIP();
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketClosed), string.Empty);
        this.liveFeedService.RemoveClient(this.WebSocket!, ipAddress);
        scopedLogger.LogDebug("Client removed from livefeed");
        return Task.CompletedTask;
    }
}

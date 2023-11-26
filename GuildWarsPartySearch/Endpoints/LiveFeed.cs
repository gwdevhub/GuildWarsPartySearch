using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.PartySearch;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

public sealed class LiveFeed : WebSocketRouteBase<None, List<Models.PartySearch>>
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

    public override Task ExecuteAsync(None? type, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override async Task SocketAccepted(CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketAccepted), this.Context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        this.liveFeedService.AddClient(this.WebSocket!);
        scopedLogger.LogInformation("Client accepted to livefeed");

        scopedLogger.LogInformation("Sending all party searches");
        var updates = await this.partySearchService.GetAllPartySearches(cancellationToken);
        await this.SendMessage(updates, cancellationToken);
    }

    public override Task SocketClosed()
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketAccepted), this.Context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        this.liveFeedService.RemoveClient(this.WebSocket!);
        scopedLogger.LogInformation("Client removed from livefeed");
        return Task.CompletedTask;
    }
}

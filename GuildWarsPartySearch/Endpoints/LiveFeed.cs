﻿using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.PartySearch;
using System.Core.Extensions;
using System.Extensions;
using System.Text;

namespace GuildWarsPartySearch.Server.Endpoints;

public sealed class LiveFeed : WebSocketRouteBase<TextContent, PartySearchList>
{
    private readonly static byte[] PongAnswer = Encoding.UTF8.GetBytes("pong");

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

    public override async Task ExecuteAsync(TextContent? content, CancellationToken cancellationToken)
    {
        if (content?.Text?.Equals("ping", StringComparison.OrdinalIgnoreCase) is true)
        {
            await this.WebSocket!.SendAsync(PongAnswer, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    public override async Task SocketAccepted(CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketAccepted), this.Context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        this.liveFeedService.AddClient(this.WebSocket!);
        scopedLogger.LogDebug("Client accepted to livefeed");

        scopedLogger.LogDebug("Sending all party searches");
        var updates = await this.partySearchService.GetAllPartySearches(cancellationToken);
        await this.SendMessage(new PartySearchList { Searches = updates }, cancellationToken);
    }

    public override Task SocketClosed()
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketAccepted), this.Context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        this.liveFeedService.RemoveClient(this.WebSocket!);
        scopedLogger.LogDebug("Client removed from livefeed");
        return Task.CompletedTask;
    }
}

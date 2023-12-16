using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.BotStatus;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.AspNetCore.Mvc;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[ServiceFilter<ApiKeyProtected>]
[ServiceFilter<UserAgentRequired>]
public sealed class PostPartySearch : WebSocketRouteBase<PostPartySearchRequest, PostPartySearchResponse>
{
    private readonly IBotStatusService botStatusService;
    private readonly ILiveFeedService liveFeedService;
    private readonly IPartySearchService partySearchService;
    private readonly ILogger<PostPartySearch> logger;

    public PostPartySearch(
        IBotStatusService botStatusService,
        ILiveFeedService liveFeedService,
        IPartySearchService partySearchService,
        ILogger<PostPartySearch> logger)
    {
        this.botStatusService = botStatusService.ThrowIfNull();
        this.liveFeedService = liveFeedService.ThrowIfNull();
        this.partySearchService = partySearchService.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public override async Task SocketAccepted(CancellationToken cancellationToken)
    {
        if (this.Context?.Items.TryGetValue(UserAgentRequired.UserAgentKey, out var userAgentValue) is not true ||
            userAgentValue is not string userAgent)
        {
            await this.WebSocket!.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, "Failed to extract user agent", cancellationToken);
            return;
        }

        if (!await this.botStatusService.AddBot(userAgent, this.WebSocket!))
        {
            await this.WebSocket!.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.PolicyViolation, $"Failed to add bot with id {userAgent}", cancellationToken);
            return;
        }
    }

    public override async Task SocketClosed()
    {
        if (this.Context?.Items.TryGetValue(UserAgentRequired.UserAgentKey, out var userAgentValue) is not true ||
            userAgentValue is not string userAgent)
        {
            throw new InvalidOperationException("Unable to extract user agent on client disconnect");
        }

        if (!await this.botStatusService.RemoveBot(userAgent))
        {
            throw new InvalidOperationException($"Failed to remove bot with id {userAgent}");
        }
    }

    public override async Task ExecuteAsync(PostPartySearchRequest? message, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.ExecuteAsync), string.Empty);
        try
        {
            var result = await this.partySearchService.PostPartySearch(message, cancellationToken);
            var response = result.Switch<PostPartySearchResponse>(
                onSuccess: _ =>
                {
                    this.liveFeedService.PushUpdate(new PartySearch
                    {
                        Campaign = message?.Campaign,
                        Continent = message?.Continent,
                        District = message?.District,
                        Map = message?.Map,
                        PartySearchEntries = message?.PartySearchEntries,
                        Region = message?.Region
                    }, cancellationToken);
                    return Success;
                },
                onFailure: failure => failure switch
                {
                    PostPartySearchFailure.InvalidPayload => InvalidPayload,
                    PostPartySearchFailure.InvalidCampaign => InvalidCampaign,
                    PostPartySearchFailure.InvalidContinent => InvalidContinent,
                    PostPartySearchFailure.InvalidRegion => InvalidRegion,
                    PostPartySearchFailure.InvalidMap => InvalidMap,
                    PostPartySearchFailure.InvalidDistrict => InvalidDistrict,
                    PostPartySearchFailure.InvalidEntries => InvalidEntries,
                    PostPartySearchFailure.InvalidPartySize => InvalidPartySize,
                    PostPartySearchFailure.InvalidPartyMaxSize => InvalidPartyMaxSize,
                    PostPartySearchFailure.InvalidNpcs => InvalidNpcs,
                    PostPartySearchFailure.InvalidCharName => InvalidCharName,
                    PostPartySearchFailure.UnspecifiedFailure => UnspecifiedFailure,
                    _ => UnspecifiedFailure
                });

            await this.SendMessage(response, cancellationToken);
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
        }
    }

    private static PostPartySearchResponse Success => new()
    {
        Result = 0,
        Description = "Posted entries"
    };

    private static PostPartySearchResponse InvalidPayload => new()
    {
        Result = 0,
        Description = "Invalid payload"
    };

    private static PostPartySearchResponse InvalidCampaign => new()
    {
        Result = 0,
        Description = "Invalid campaign"
    };

    private static PostPartySearchResponse InvalidContinent => new()
    {
        Result = 0,
        Description = "Invalid continent"
    };

    private static PostPartySearchResponse InvalidRegion => new()
    {
        Result = 0,
        Description = "Invalid region"
    };

    private static PostPartySearchResponse InvalidMap => new()
    {
        Result = 0,
        Description = "Invalid map"
    };

    private static PostPartySearchResponse InvalidDistrict => new()
    {
        Result = 0,
        Description = "Invalid district"
    };

    private static PostPartySearchResponse InvalidEntries => new()
    {
        Result = 0,
        Description = "Invalid entries"
    };

    private static PostPartySearchResponse InvalidPartySize => new()
    {
        Result = 0,
        Description = "Invalid party size"
    };

    private static PostPartySearchResponse InvalidPartyMaxSize => new()
    {
        Result = 0,
        Description = "Invalid max party size"
    };

    private static PostPartySearchResponse InvalidNpcs => new()
    {
        Result = 0,
        Description = "Invalid npcs"
    };

    private static PostPartySearchResponse InvalidCharName => new()
    {
        Result = 0,
        Description = "Invalid char name"
    };

    private static PostPartySearchResponse UnspecifiedFailure => new()
    {
        Result = 0,
        Description = "Unspecified failure"
    };
}

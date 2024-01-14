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

[ServiceFilter<IpWhitelistFilter>]
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
                        Map = message?.Map,
                        DistrictLanguage = message?.DistrictLanguage,
                        DistrictNumber = message?.DistrictNumber ?? 0,
                        DistrictRegion = message?.DistrictRegion,
                        PartySearchEntries = message?.PartySearchEntries,
                    }, cancellationToken);
                    return Success;
                },
                onFailure: failure => failure switch
                {
                    PostPartySearchFailure.InvalidPayload => InvalidPayload,
                    PostPartySearchFailure.InvalidMap => InvalidMap,
                    PostPartySearchFailure.InvalidEntries => InvalidEntries,
                    PostPartySearchFailure.InvalidSender => InvalidSender,
                    PostPartySearchFailure.InvalidMessage => InvalidMessage,
                    PostPartySearchFailure.InvalidDistrictRegion => InvalidDistrictRegion,
                    PostPartySearchFailure.InvalidDistrictLanguage => InvalidDistrictLanguage,
                    PostPartySearchFailure.InvalidDistrictNumber => InvalidDistrictNumber,
                    PostPartySearchFailure.InvalidHeroCount => InvalidHeroCount,
                    PostPartySearchFailure.InvalidHardMode => InvalidHardMode,
                    PostPartySearchFailure.InvalidSearchType => InvalidSearchType,
                    PostPartySearchFailure.InvalidPrimary => InvalidPrimary,
                    PostPartySearchFailure.InvalidSecondary => InvalidSecondary,
                    PostPartySearchFailure.InvalidLevel => InvalidLevel,
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

    private static PostPartySearchResponse InvalidMap => new()
    {
        Result = 0,
        Description = "Invalid map"
    };

    private static PostPartySearchResponse InvalidEntries => new()
    {
        Result = 0,
        Description = "Invalid entries"
    };

    private static PostPartySearchResponse InvalidSender => new()
    {
        Result = 0,
        Description = "Invalid sender"
    };

    private static PostPartySearchResponse InvalidMessage => new()
    {
        Result = 0,
        Description = "Invalid message"
    };

    private static PostPartySearchResponse InvalidDistrictRegion => new()
    {
        Result = 0,
        Description = "Invalid district region"
    };

    private static PostPartySearchResponse InvalidDistrictLanguage => new()
    {
        Result = 0,
        Description = "Invalid district language"
    };

    private static PostPartySearchResponse InvalidDistrictNumber => new()
    {
        Result = 0,
        Description = "Invalid district number"
    };

    private static PostPartySearchResponse InvalidPartyId => new()
    {
        Result = 0,
        Description = "Invalid party id"
    };

    private static PostPartySearchResponse InvalidHeroCount => new()
    {
        Result = 0,
        Description = "Invalid hero count"
    };

    private static PostPartySearchResponse InvalidHardMode => new()
    {
        Result = 0,
        Description = "Invalid hard mode"
    };

    private static PostPartySearchResponse InvalidSearchType => new()
    {
        Result = 0,
        Description = "Invalid search type"
    };

    private static PostPartySearchResponse InvalidPrimary => new()
    {
        Result = 0,
        Description = "Invalid primary"
    };

    private static PostPartySearchResponse InvalidSecondary => new()
    {
        Result = 0,
        Description = "Invalid secondary"
    };

    private static PostPartySearchResponse InvalidLevel => new()
    {
        Result = 0,
        Description = "Invalid level"
    };

    private static PostPartySearchResponse UnspecifiedFailure => new()
    {
        Result = 0,
        Description = "Unspecified failure"
    };
}

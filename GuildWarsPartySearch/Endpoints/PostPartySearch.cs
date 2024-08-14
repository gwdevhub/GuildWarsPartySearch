﻿using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.BotStatus;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.AspNetCore.Mvc;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[ServiceFilter<BotPermissionRequired>]
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
		var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketAccepted), string.Empty);
		if (this.Context?.Items.TryGetValue(UserAgentRequired.UserAgentKey, out var userAgentValue) is not true ||
            userAgentValue is not string userAgent)
        {
			scopedLogger.LogDebug("Failed to extract user agent");
			await this.WebSocket!.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, "Failed to extract user agent", cancellationToken);
            return;
        }

        if (!await this.botStatusService.AddBot(userAgent, this.WebSocket!, cancellationToken))
        {
			scopedLogger.LogDebug($"Failed to add bot with id {userAgent}");
			await this.WebSocket!.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.PolicyViolation, $"Failed to add bot with id {userAgent}", cancellationToken);
            return;
        }
    }

    public override async Task SocketClosed()
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SocketClosed), string.Empty);
        if (this.Context?.Items.TryGetValue(UserAgentRequired.UserAgentKey, out var userAgentValue) is not true ||
            userAgentValue is not string userAgent)
        {

			scopedLogger.LogDebug("No user agent found. A connection has been rejected");
            return;
        }

        if (!await this.botStatusService.RemoveBot(userAgent, CancellationToken.None))
        {
            throw new InvalidOperationException($"Failed to remove bot with id {userAgent}");
        }
    }

    public override async Task ExecuteAsync(PostPartySearchRequest? message, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.ExecuteAsync), string.Empty);
        try
        {
            if (this.Context?.Items.TryGetValue(UserAgentRequired.UserAgentKey, out var userAgentValue) is not true ||
                userAgentValue is not string userAgent)
            {
                throw new InvalidOperationException("Unable to extract user agent on client disconnect");
            }

            await this.botStatusService.RecordBotUpdateActivity(userAgent, this.Context.RequestAborted);
            var result = await this.partySearchService.PostPartySearch(message, cancellationToken);
            var response = await result.Switch<Task<PostPartySearchResponse>>(
                onSuccess: async parsedResult =>
                {
                    await this.liveFeedService.PushUpdate(new PartySearch
                    {
                        Map = parsedResult?.Map,
                        District = parsedResult?.District ?? 0,
                        PartySearchEntries = parsedResult?.PartySearchEntries?.Select(e =>
                        {
                            // Patch the input to match the original district
                            e.District = parsedResult.District ?? 0;
                            return e;
                        }).ToList(),
                    }, cancellationToken);

                    var response = Success.Result;
                    if (parsedResult?.GetFullList is true)
                    {
                        var currentBot = await this.botStatusService.GetBot(userAgent, cancellationToken);
                        if (currentBot is null)
                        {
                            scopedLogger.LogError($"Failed to find bot for id {userAgent}");
                        }

                        var party_searches = await this.partySearchService.GetAllPartySearches(cancellationToken);
                        
                        // Return all party searches besides the current one
                        response.PartySearches = party_searches
                        .Where(p => p.Map != currentBot?.Map || p.District != currentBot?.District)
                        .Select(p => new PartySearch { Map = p.Map, District = p.District }).ToList();
                    }
                    return response;
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

    private static Task<PostPartySearchResponse> Success => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Posted entries"
    });

    private static Task<PostPartySearchResponse> InvalidPayload => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid payload"
    });

    private static Task<PostPartySearchResponse> InvalidMap => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid map"
    });

    private static Task<PostPartySearchResponse> InvalidEntries => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid entries"
    });

    private static Task<PostPartySearchResponse> InvalidSender => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid sender"
    });

    private static Task<PostPartySearchResponse> InvalidMessage => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid message"
    });

    private static Task<PostPartySearchResponse> InvalidDistrictRegion => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid district region"
    });

    private static Task<PostPartySearchResponse> InvalidDistrictLanguage => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid district language"
    });

    private static Task<PostPartySearchResponse> InvalidDistrictNumber => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid district number"
    });

    private static Task<PostPartySearchResponse> InvalidPartyId => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid party id"
    });

    private static Task<PostPartySearchResponse> InvalidHeroCount => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid hero count"
    });

    private static Task<PostPartySearchResponse> InvalidHardMode => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid hard mode"
    });

    private static Task<PostPartySearchResponse> InvalidSearchType => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid search type"
    });

    private static Task<PostPartySearchResponse> InvalidPrimary => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid primary"
    });

    private static Task<PostPartySearchResponse> InvalidSecondary => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid secondary"
    });

    private static Task<PostPartySearchResponse> InvalidLevel => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Invalid level"
    });

    private static Task<PostPartySearchResponse> UnspecifiedFailure => Task.FromResult(new PostPartySearchResponse
    {
        Result = 0,
        Description = "Unspecified failure"
    });
}

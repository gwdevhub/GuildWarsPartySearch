using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.Extensions.Logging;
using MTSC.Common.Http;
using MTSC.Common.Http.Attributes;
using MTSC.Common.Http.RoutingModules;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[SimpleStringTokenFilter]
[ReturnBadRequestOnDataBindingFailure]
public sealed class PostPartySearch : HttpRouteBase<None>
{
    [FromBody]
    public PostPartySearchRequest? Payload { get; init; }

    private readonly IPartySearchService partySearchService;
    private readonly ILogger<PostPartySearch> logger;

    public PostPartySearch(
        IPartySearchService partySearchService,
        ILogger<PostPartySearch> logger)
    {
        this.partySearchService = partySearchService.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public override async Task<HttpResponse> HandleRequest(None request)
    {
        var result = await this.partySearchService.PostPartySearch(this.Payload);
        return result.Switch<HttpResponse>(
            onSuccess: _ => Success200,
            onFailure: failure => failure switch
            {
                PostPartySearchFailure.InvalidPayload => NoPayload400,
                PostPartySearchFailure.InvalidCampaign => InvalidCampaign400,
                PostPartySearchFailure.InvalidContinent => InvalidContinent400,
                PostPartySearchFailure.InvalidRegion => InvalidRegion400,
                PostPartySearchFailure.InvalidMap => InvalidMap400,
                PostPartySearchFailure.InvalidDistrict => InvalidDistrict400,
                PostPartySearchFailure.InvalidEntries => InvalidPartySearchEntries400,
                PostPartySearchFailure.InvalidPartySize => InvalidPartySize400,
                PostPartySearchFailure.InvalidPartyMaxSize => InvalidPartyMaxSize400,
                PostPartySearchFailure.InvalidNpcs => InvalidNpcs400,
                PostPartySearchFailure.UnspecifiedFailure => UnspecifiedFailure500,
                _ => UnspecifiedFailure500
            });
    }

    private static HttpResponse Success200 => new()
    {
        StatusCode = HttpMessage.StatusCodes.OK,
        BodyString = "Party search posted"
    };

    private static HttpResponse NoPayload400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "No payload"
    };

    private static HttpResponse InvalidCampaign400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid Campaign"
    };

    private static HttpResponse InvalidContinent400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid Continent"
    };

    private static HttpResponse InvalidRegion400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid Region"
    };

    private static HttpResponse InvalidMap400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid Map"
    };

    private static HttpResponse InvalidDistrict400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid District"
    };

    private static HttpResponse InvalidPartySearchEntries400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid PartySearchEntries"
    };

    private static HttpResponse InvalidPartySize400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid PartySize"
    };

    private static HttpResponse InvalidPartyMaxSize400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid PartyMaxSize"
    };

    private static HttpResponse InvalidNpcs400 => new()
    {
        StatusCode = HttpMessage.StatusCodes.BadRequest,
        BodyString = "Invalid Npcs"
    };

    private static HttpResponse UnspecifiedFailure500 => new()
    {
        StatusCode = HttpMessage.StatusCodes.InternalServerError,
        BodyString = "Encountered an error while processing request"
    };
}

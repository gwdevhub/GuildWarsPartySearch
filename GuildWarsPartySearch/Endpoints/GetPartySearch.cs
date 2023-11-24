using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.Extensions.Logging;
using MTSC.Common.Http;
using MTSC.Common.Http.Attributes;
using MTSC.Common.Http.RoutingModules;
using Newtonsoft.Json;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[DecodeUrlFilter]
[SimpleStringTokenFilter]
[ReturnBadRequestOnDataBindingFailure]
public sealed class GetPartySearch : HttpRouteBase<None>
{
    private readonly IPartySearchService partySearchService;
    private readonly ILogger<GetPartySearch> logger;

    [FromUrl("campaign")]
    public Campaign? Campaign { get; init; }

    [FromUrl("continent")]
    public Continent? Continent { get; init; }

    [FromUrl("region")]
    public Region? Region { get; init; }

    [FromUrl("map")]
    public Map? Map { get; init; }

    [FromUrl("district")]
    public string? District { get; init; }

    public GetPartySearch(
        IPartySearchService partySearchService,
        ILogger<GetPartySearch> logger)
    {
        this.partySearchService = partySearchService.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public override async Task<HttpResponse> HandleRequest(None request)
    {
        var result = await this.partySearchService.GetPartySearch(this.Campaign, this.Continent, this.Region, this.Map, this.District);
        return result.Switch<HttpResponse>(
            onSuccess: entries => Success200(entries),
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidPayload => NoPayload400,
                GetPartySearchFailure.InvalidCampaign => InvalidCampaign400,
                GetPartySearchFailure.InvalidContinent => InvalidContinent400,
                GetPartySearchFailure.InvalidRegion => InvalidRegion400,
                GetPartySearchFailure.InvalidMap => InvalidMap400,
                GetPartySearchFailure.InvalidDistrict => InvalidDistrict400,
                GetPartySearchFailure.EntriesNotFound => EntriesNotFound404,
                GetPartySearchFailure.UnspecifiedFailure => UnspecifiedFailure500,
                _ => UnspecifiedFailure500
            });
    }

    private static HttpResponse Success200(List<PartySearchEntry> entries) => new()
    {
        StatusCode = HttpMessage.StatusCodes.OK,
        BodyString = JsonConvert.SerializeObject(entries)
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

    private static HttpResponse EntriesNotFound404 => new()
    {
        StatusCode = HttpMessage.StatusCodes.NotFound,
        BodyString = "Entries not found"
    };

    private static HttpResponse UnspecifiedFailure500 => new()
    {
        StatusCode = HttpMessage.StatusCodes.InternalServerError,
        BodyString = "Encountered an error while processing request"
    };
}

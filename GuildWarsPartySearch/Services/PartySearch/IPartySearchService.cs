using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models.Endpoints;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.PartySearch;

public interface IPartySearchService
{
    Task<Result<PostPartySearchRequest, PostPartySearchFailure>> PostPartySearch(PostPartySearchRequest? request);

    Task<Result<List<PartySearchEntry>, GetPartySearchFailure>> GetPartySearch(Campaign? campaign, Continent? continent, Region? region, Map? map, string? district);
}

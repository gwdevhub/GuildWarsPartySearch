using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Models.Endpoints;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.PartySearch;

public interface IPartySearchService
{
    Task<Result<List<Models.PartySearch>, GetPartySearchFailure>> GetPartySearchesByCharName(string charName, CancellationToken cancellationToken);

    Task<List<Models.PartySearch>> GetAllPartySearches(CancellationToken cancellationToken);

    Task<Result<PostPartySearchRequest, PostPartySearchFailure>> PostPartySearch(PostPartySearchRequest? request, CancellationToken cancellationToken);

    Task<Result<List<PartySearchEntry>, GetPartySearchFailure>> GetPartySearch(Campaign? campaign, Continent? continent, Region? region, Map? map, string? district, CancellationToken cancellationToken);
}

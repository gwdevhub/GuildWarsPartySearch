using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models.Endpoints;

namespace GuildWarsPartySearch.Server.Services.Database;

public interface IPartySearchDatabase
{
    Task<List<PartySearchUpdate>> GetAllPartySearches();
    Task<bool> SetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district, List<Models.Database.PartySearch> partySearch);
    Task<List<Models.Database.PartySearch>?> GetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district);
}

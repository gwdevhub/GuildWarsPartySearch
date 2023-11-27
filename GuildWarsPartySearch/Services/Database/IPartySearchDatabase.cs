using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Services.Database;

public interface IPartySearchDatabase
{
    Task<List<Server.Models.PartySearch>> GetPartySearchesByCampaign(Campaign campaign, CancellationToken cancellationToken);
    Task<List<Server.Models.PartySearch>> GetPartySearchesByContinent(Continent continent, CancellationToken cancellationToken);
    Task<List<Server.Models.PartySearch>> GetPartySearchesByRegion(Region region, CancellationToken cancellationToken);
    Task<List<Server.Models.PartySearch>> GetPartySearchesByMap(Map map, CancellationToken cancellationToken);
    Task<List<Server.Models.PartySearch>> GetPartySearchesByCharName(string charName, CancellationToken cancellationToken);
    Task<List<Server.Models.PartySearch>> GetAllPartySearches(CancellationToken cancellationToken);
    Task<bool> SetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district, List<PartySearchEntry> partySearch, CancellationToken cancellationToken);
    Task<List<PartySearchEntry>?> GetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district, CancellationToken cancellationToken);
}

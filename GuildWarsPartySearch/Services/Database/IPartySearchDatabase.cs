using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Services.Database;

public interface IPartySearchDatabase
{
    Task<List<Server.Models.PartySearch>> GetPartySearchesByMap(Map map, CancellationToken cancellationToken);
    Task<List<Server.Models.PartySearch>> GetPartySearchesByCharName(string charName, CancellationToken cancellationToken);
    Task<List<Server.Models.PartySearch>> GetAllPartySearches(CancellationToken cancellationToken);
    Task<bool> SetPartySearches(Map map, int district, List<PartySearchEntry> partySearch, CancellationToken cancellationToken);
    Task<List<PartySearchEntry>?> GetPartySearches(Map map, int district, CancellationToken cancellationToken);
}

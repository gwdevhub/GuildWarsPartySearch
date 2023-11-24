using GuildWarsPartySearch.Common.Models.GuildWars;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class InMemoryPartySearchDatabase : IPartySearchDatabase
{
    private static readonly ConcurrentDictionary<string, List<Models.Database.PartySearch>> partySearchCache = new();

    private readonly ILogger<InMemoryPartySearchDatabase> logger;

    public InMemoryPartySearchDatabase(
        ILogger<InMemoryPartySearchDatabase> logger)
    {
        this.logger = logger.ThrowIfNull();
    }

    public Task<bool> SetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district, List<Models.Database.PartySearch> partySearch)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(SetPartySearches), string.Empty);
        var key = BuildKey(campaign, continent, region, map, district);
        partySearchCache[key] = partySearch;
        scopedLogger.LogInformation($"Set cache for {key}");
        return Task.FromResult(true);
    }

    public Task<List<Models.Database.PartySearch>?> GetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district)
    {
        district = district.Replace("%20", " ");
        var scopedLogger = this.logger.CreateScopedLogger(nameof(GetPartySearches), string.Empty);
        var key = BuildKey(campaign, continent, region, map, district);
        if (!partySearchCache.TryGetValue(key, out var partySearch))
        {
            return Task.FromResult<List<Models.Database.PartySearch>?>(default);
        }

        return Task.FromResult<List<Models.Database.PartySearch>?>(partySearch);
    }

    private static string BuildKey(Campaign campaign, Continent continent, Region region, Map map, string district)
    {
        return $"{campaign.Name};{continent.Name};{region.Name};{map.Name};{district}";
    }

    
}

using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models.Endpoints;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class InMemoryPartySearchDatabase : IPartySearchDatabase
{
    private static readonly ConcurrentDictionary<(Campaign Campaign, Continent Continent, Region Region, Map Map, string District), List<Models.Database.PartySearch>> PartySearchCache = new();

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
        PartySearchCache[key] = partySearch;
        scopedLogger.LogInformation($"Set cache for {key}");
        return Task.FromResult(true);
    }

    public Task<List<Models.Database.PartySearch>?> GetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district)
    {
        district = district.Replace("%20", " ");
        var scopedLogger = this.logger.CreateScopedLogger(nameof(GetPartySearches), string.Empty);
        var key = BuildKey(campaign, continent, region, map, district);
        if (!PartySearchCache.TryGetValue(key, out var partySearch))
        {
            return Task.FromResult<List<Models.Database.PartySearch>?>(default);
        }

        return Task.FromResult<List<Models.Database.PartySearch>?>(partySearch);
    }

    public Task<List<PartySearchUpdate>> GetAllPartySearches()
    {
        return Task.FromResult(PartySearchCache.Select(t =>
        {
            return new PartySearchUpdate
            {
                Campaign = t.Key.Campaign,
                Continent = t.Key.Continent,
                Region = t.Key.Region,
                Map = t.Key.Map,
                District = t.Key.District,
                PartySearchEntries = t.Value.Select(e => new PartySearchEntry
                {
                    Npcs = e.Npcs,
                    PartyMaxSize = e.PartyMaxSize,
                    PartySize = e.PartySize,
                }).ToList()
            };
        }).ToList());
    }

    private static (Campaign Campaign, Continent Continent, Region Region, Map Map, string District) BuildKey(Campaign campaign, Continent continent, Region region, Map map, string district)
    {
        return (campaign, continent, region, map, district);
    }
}

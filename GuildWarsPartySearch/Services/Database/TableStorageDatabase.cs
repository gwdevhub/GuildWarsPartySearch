﻿using Azure.Data.Tables;
using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Azure;
using GuildWarsPartySearch.Server.Services.Database.Models;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class TableStorageDatabase : IPartySearchDatabase
{
    private readonly NamedTableClient<PartySearchTableOptions> client;
    private readonly ILogger<TableStorageDatabase> logger;

    public TableStorageDatabase(
        NamedTableClient<PartySearchTableOptions> namedTableClient,
        ILogger<TableStorageDatabase> logger)
    {
        this.client = namedTableClient.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public async Task<List<Server.Models.PartySearch>> GetAllPartySearches(CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetAllPartySearches), string.Empty);
        try
        {
            var response = await this.QuerySearches(string.Empty, cancellationToken);
            return response;
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<List<Server.Models.PartySearch>> GetPartySearchesByCampaign(Campaign campaign, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearchesByCampaign), string.Empty);
        try
        {
            return await this.QuerySearches($"{nameof(PartySearchTableEntity.Campaign)} eq '{campaign.Name?.Replace("'", "''")}'", cancellationToken);
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<List<Server.Models.PartySearch>> GetPartySearchesByContinent(Continent continent, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearchesByContinent), string.Empty);
        try
        {
            return await this.QuerySearches($"{nameof(PartySearchTableEntity.Continent)} eq '{continent.Name?.Replace("'", "''")}'", cancellationToken);
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<List<Server.Models.PartySearch>> GetPartySearchesByRegion(Region region, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearchesByRegion), string.Empty);
        try
        {
            return await this.QuerySearches($"{nameof(PartySearchTableEntity.Region)} eq '{region.Name?.Replace("'", "''")}'", cancellationToken);
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<List<Server.Models.PartySearch>> GetPartySearchesByMap(Map map, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearchesByMap), string.Empty);
        try
        {
            return await this.QuerySearches($"{nameof(PartySearchTableEntity.Map)} eq '{map.Name?.Replace("'", "''")}'", cancellationToken);
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<List<Server.Models.PartySearch>> GetPartySearchesByCharName(string charName, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearchesByCharName), string.Empty);
        try
        {
            return await this.QuerySearches($"{nameof(PartySearchTableEntity.CharName)} eq '{charName.Replace("'", "''")}'", cancellationToken);
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<List<PartySearchEntry>?> GetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district, CancellationToken cancellationToken)
    {
        var partitionKey = BuildPartitionKey(campaign, continent, region, map, district);
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearches), partitionKey);
        try
        {
            var response = await this.QuerySearches($"PartitionKey eq '{partitionKey.Replace("'", "''")}'", cancellationToken);
            var partition = response.FirstOrDefault();
            if (partition is null)
            {
                return default;
            }

            return partition.PartySearchEntries;
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
        }

        return default;
    }

    public async Task<bool> SetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district, List<PartySearchEntry> partySearch, CancellationToken cancellationToken)
    {
        var partitionKey = BuildPartitionKey(campaign, continent, region, map, district);
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SetPartySearches), partitionKey);
        try
        {
            var existingEntries = await this.GetPartySearches(campaign, continent, region, map, district, cancellationToken);
            var entries = partySearch.Select(e =>
            {
                var rowKey = e.CharName ?? string.Empty;
                return new PartySearchTableEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    Campaign = campaign.Name,
                    Continent = continent.Name,
                    Region = region.Name,
                    Map = map.Name,
                    District = district,
                    CharName = e.CharName,
                    PartySize = e.PartySize,
                    PartyMaxSize = e.PartyMaxSize,
                    Npcs = e.Npcs,
                    Timestamp = DateTimeOffset.UtcNow
                };
            });

            var actions = new List<TableTransactionAction>();
            if (existingEntries is not null)
            {
                // Find all existing entries that don't exist in the update. For those, queue a delete transaction
                actions.AddRange(existingEntries
                    .Where(e => entries.FirstOrDefault(e2 => e.CharName == e2.CharName) is null)
                    .Select(e => new TableTransactionAction(TableTransactionActionType.Delete, new PartySearchTableEntity
                    {
                        PartitionKey = partitionKey,
                        RowKey = e.CharName ?? string.Empty,
                    })));
            }

            actions.AddRange(entries
                .Where(e =>
                {
                    // Only update entries that have changed
                    var existingEntry = entries.FirstOrDefault(e2 => e2.RowKey == e.RowKey);
                    return e.Campaign != existingEntry?.Campaign ||
                            e.Continent != existingEntry?.Continent ||
                            e.Region != existingEntry?.Region ||
                            e.Map != existingEntry?.Map ||
                            e.District != existingEntry?.District ||
                            e.CharName != existingEntry?.CharName ||
                            e.PartySize != existingEntry?.PartySize ||
                            e.PartyMaxSize != existingEntry?.PartyMaxSize ||
                            e.Npcs != existingEntry?.Npcs;
                })
                .Select(e => new TableTransactionAction(TableTransactionActionType.UpsertReplace, e)));
            if (actions.None())
            {
                scopedLogger.LogInformation("No change detected. Skipping operation");
                return true;
            }

            var responses = await this.client.SubmitTransactionAsync(actions, cancellationToken);
            foreach(var response in responses.Value)
            {
                scopedLogger.LogInformation($"[{response.Status}] {response.ReasonPhrase}");
            }

            return responses.Value.None(r => r.IsError);
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while setting party searches");
            return false;
        }
    }

    private async Task<List<Server.Models.PartySearch>> QuerySearches(string query, CancellationToken cancellationToken)
    {
        var responseList = new Dictionary<string, ((Campaign, Continent, Region, Map, string), List<PartySearchEntry>)>();
        var response = this.client.QueryAsync<PartySearchTableEntity>(query, cancellationToken: cancellationToken);
        await foreach (var entry in response)
        {
            if (!responseList.TryGetValue(entry.PartitionKey, out var tuple))
            {
                Campaign.TryParse(entry.Campaign!, out var campaign);
                Continent.TryParse(entry.Continent!, out var continent);
                Region.TryParse(entry.Region!, out var region);
                Map.TryParse(entry.Map!, out var map);
                tuple = ((campaign!, continent!, region!, map!, entry.District!), new List<PartySearchEntry>());
                responseList[entry.PartitionKey] = tuple;
            }

            tuple.Item2.Add(new PartySearchEntry
            {
                CharName = entry.CharName,
                Npcs = entry.Npcs,
                PartyMaxSize = entry.PartyMaxSize,
                PartySize = entry.PartySize
            });
        }

        return responseList.Values.Select(tuple => new Server.Models.PartySearch
        {
            Campaign = tuple.Item1.Item1,
            Continent = tuple.Item1.Item2,
            Region = tuple.Item1.Item3,
            Map = tuple.Item1.Item4,
            District = tuple.Item1.Item5,
            PartySearchEntries = tuple.Item2
        }).ToList();
    }

    private static string BuildPartitionKey(Campaign campaign, Continent continent, Region region, Map map, string district)
    {
        return $"{campaign.Name};{continent.Name};{region.Name};{map.Name};{district}";
    }
}

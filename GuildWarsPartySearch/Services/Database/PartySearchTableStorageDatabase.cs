using Azure.Data.Tables;
using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Azure;
using GuildWarsPartySearch.Server.Services.Database.Models;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class PartySearchTableStorageDatabase : IPartySearchDatabase
{
    private readonly NamedTableClient<PartySearchTableOptions> client;
    private readonly ILogger<PartySearchTableStorageDatabase> logger;

    public PartySearchTableStorageDatabase(
        NamedTableClient<PartySearchTableOptions> namedTableClient,
        ILogger<PartySearchTableStorageDatabase> logger)
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

    public async Task<List<Server.Models.PartySearch>> GetPartySearchesByMap(Map map, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearchesByMap), string.Empty);
        try
        {
            return await this.QuerySearches($"{nameof(PartySearchTableEntity.MapId)} eq {map.Id}", cancellationToken);
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
            return await this.QuerySearches($"{nameof(PartySearchTableEntity.Sender)} eq '{charName.Replace("'", "''")}'", cancellationToken);
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<bool> SetPartySearches(Map map, List<PartySearchEntry> partySearch, CancellationToken cancellationToken)
    {
        var partitionKey = BuildPartitionKey(map);
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SetPartySearches), partitionKey);
        try
        {
            var existingEntries = await this.GetPartySearches(map, cancellationToken);
            var entries = partySearch.Select(e =>
            {
                var rowKey = e.Sender ?? string.Empty;
                return new PartySearchTableEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    DistrictLanguage = (int?)e.DistrictLanguage ?? 0,
                    DistrictNumber = e.DistrictNumber,
                    Message = e.Message,
                    Sender = e.Sender,
                    PartyId = e.PartyId,
                    PartySize = e.PartySize,
                    HardMode = (int)(e.HardMode ?? HardModeState.Disabled),
                    HeroCount = e.HeroCount,
                    Level = e.Level,
                    MapId = map.Id,
                    Primary = e.Primary?.Id ?? 0,
                    Secondary = e.Secondary?.Id ?? 0,
                    SearchType = e.SearchType,
                    Timestamp = DateTimeOffset.UtcNow
                };
            });

            var actions = new List<TableTransactionAction>();
            if (existingEntries is not null)
            {
                // Find all existing entries that don't exist in the update. For those, queue a delete transaction
                actions.AddRange(existingEntries
                    .Where(e => entries.FirstOrDefault(e2 => e.Sender == e2.Sender) is null)
                    .Select(e => new TableTransactionAction(TableTransactionActionType.Delete, new PartySearchTableEntity
                    {
                        PartitionKey = partitionKey,
                        RowKey = e.Sender ?? string.Empty,
                    })));
            }

            actions.AddRange(entries
                .Where(e =>
                {
                    // Only update entries that have changed
                    var existingEntry = existingEntries?.FirstOrDefault(e2 => e2.Sender == e.Sender);
                    return existingEntry is null ||
                            e.Sender != existingEntry?.Sender ||
                            e.PartySize != existingEntry?.PartySize ||
                            e.PartyId != existingEntry?.PartyId ||
                            e.Primary != existingEntry?.Primary?.Id ||
                            e.Secondary != existingEntry?.Secondary?.Id ||
                            e.HardMode != (int)(existingEntry?.HardMode ?? 0) ||
                            e.Level != existingEntry?.Level ||
                            e.Message != existingEntry.Message ||
                            e.DistrictLanguage != (int)(existingEntry?.DistrictLanguage ?? DistrictLanguage.English) ||
                            e.DistrictNumber != existingEntry?.DistrictNumber;
                })
                .Select(e => new TableTransactionAction(TableTransactionActionType.UpsertReplace, e)));
            if (actions.None())
            {
                scopedLogger.LogInformation("No change detected. Skipping operation");
                return true;
            }

            var responses = await this.client.SubmitTransactionAsync(actions, cancellationToken);
            foreach (var response in responses.Value)
            {
                scopedLogger.LogInformation($"[{response.Status}] {response.ReasonPhrase}");
            }

            return responses.Value.None(r => r.IsError);
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while setting party searches");
            return false;
        }
    }

    public async Task<List<PartySearchEntry>?> GetPartySearches(Map map, CancellationToken cancellationToken)
    {
        var partitionKey = BuildPartitionKey(map);
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
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
        }

        return default;
    }

    private async Task<List<Server.Models.PartySearch>> QuerySearches(string query, CancellationToken cancellationToken)
    {
        var responseList = new Dictionary<string, (Map, List<PartySearchEntry>)>();
        var response = this.client.QueryAsync<PartySearchTableEntity>(query, cancellationToken: cancellationToken);
        await foreach (var entry in response)
        {
            if (!responseList.TryGetValue(entry.PartitionKey, out var tuple))
            {
                _ = Map.TryParse(entry.MapId!, out var map);
                var districtLanguage = (DistrictLanguage)entry.DistrictLanguage;
                tuple = (map, new List<PartySearchEntry>());
                responseList[entry.PartitionKey] = tuple;
            }

            _ = Profession.TryParse(entry.Primary, out var primary);
            _ = Profession.TryParse(entry.Secondary, out var secondary);
            tuple.Item2.Add(new PartySearchEntry
            {
                PartyId = entry.PartyId,
                DistrictNumber = entry.DistrictNumber,
                DistrictLanguage = (DistrictLanguage)entry.DistrictLanguage,
                Message = entry.Message,
                Sender = entry.Sender,
                PartySize = entry.PartySize,
                HeroCount = entry.HeroCount,
                HardMode = (HardModeState)entry.HardMode,
                SearchType = entry.SearchType,
                Primary = primary,
                Secondary = secondary,
                Level = entry.Level
            });
        }

        return responseList.Values.Select(tuple => new Server.Models.PartySearch
        {
            Map = tuple.Item1,
            PartySearchEntries = tuple.Item2
        }).ToList();
    }

    private static string BuildPartitionKey(Map map)
    {
        return $"{map.Id}";
    }
}

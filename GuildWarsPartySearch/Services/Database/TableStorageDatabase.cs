using Azure.Data.Tables;
using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class TableStorageDatabase : IPartySearchDatabase
{
    private readonly TableClient tableClient;
    private readonly IOptions<StorageAccountOptions> options;
    private readonly ILogger<TableStorageDatabase> logger;

    public TableStorageDatabase(
        IOptions<StorageAccountOptions> options,
        ILogger<TableStorageDatabase> logger)
    {
        var tableClientOptions = new TableClientOptions();
        tableClientOptions.Diagnostics.IsLoggingEnabled = true;

        this.options = options.ThrowIfNull();
        this.logger = logger.ThrowIfNull();

        var tableServiceClient = new TableServiceClient(this.options?.Value.ConnectionString ?? throw new InvalidOperationException("Config contains no connection string"));
        var tableName = options.Value.TableName?.ThrowIfNull() ?? throw new InvalidOperationException("Config contains no table name");
        this.tableClient = tableServiceClient.GetTableClient(tableName);
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
            return new List<Server.Models.PartySearch>();
        }
    }

    public async Task<List<PartySearchEntry>?> GetPartySearches(Campaign campaign, Continent continent, Region region, Map map, string district, CancellationToken cancellationToken)
    {
        var partitionKey = BuildPartitionKey(campaign, continent, region, map, district);
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearches), partitionKey);
        try
        {
            var response = await this.QuerySearches($"PartitionKey eq '{partitionKey}'", cancellationToken);
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

            scopedLogger.LogInformation("Batch transaction");
            var transactions = entries.Select(e => new TableTransactionAction(TableTransactionActionType.UpsertReplace, e));
            var responses = await this.tableClient.SubmitTransactionAsync(transactions, cancellationToken);
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
        var response = this.tableClient.QueryAsync<PartySearchTableEntity>(query, cancellationToken: cancellationToken);
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

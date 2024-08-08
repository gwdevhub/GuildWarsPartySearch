using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public class PartySearchSqliteDatabase : IPartySearchDatabase
{
    private static readonly SemaphoreSlim TableSemaphore = new(1);

    private readonly SqliteConnection connection;
    private readonly PartySearchDatabaseOptions options;
    private readonly ILogger<PartySearchSqliteDatabase> logger;
    private readonly ValueCache<List<Server.Models.PartySearch>> allPartiesCache;

    public PartySearchSqliteDatabase(
        SqliteConnection sqliteConnection,
        IOptions<PartySearchDatabaseOptions> options,
        ILogger<PartySearchSqliteDatabase> logger)
    {
        this.connection = sqliteConnection.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
        this.options = options.ThrowIfNull().Value.ThrowIfNull();
        this.allPartiesCache = new ValueCache<List<Server.Models.PartySearch>>(this.GetAllPartySearchesInternal, TimeSpan.MaxValue);
        EnsureTableExists(this.connection, this.options.TableName);
    }

    public async Task<List<Server.Models.PartySearch>> GetAllPartySearches(CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetAllPartySearches), string.Empty);
        try
        {
            var response = await this.allPartiesCache.GetValue();
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
            return (await this.allPartiesCache.GetValue())
                .Where(p => map.Id == p.Map?.Id)
                .ToList();
        }
        catch (Exception e)
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
            return (await this.allPartiesCache.GetValue())
                .Where(p => p.PartySearchEntries?.Any(e => e.Sender == charName) is true)
                .ToList();
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
            return [];
        }
    }

    public async Task<bool> SetPartySearches(Map map, int district, List<PartySearchEntry> partySearch, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.SetPartySearches), string.Empty);
        try
        {
            var existingEntries = await this.GetPartySearches(map, district, cancellationToken);
            var entries = partySearch.Select(e =>
            {
                var rowKey = e.Sender ?? string.Empty;
                return new PartySearchTableEntity
                {
                    DistrictLanguage = (int?)e.DistrictLanguage ?? 0,
                    DistrictNumber = e.DistrictNumber,
                    District = district,
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

            using var transaction = this.connection.BeginTransaction();
            var actions = new List<SqliteCommand>();
            if (existingEntries is not null)
            {
                // Find all existing entries that don't exist in the update. For those, queue a delete transaction
                actions.AddRange(existingEntries
                    .Where(e => entries.FirstOrDefault(e2 => e.Sender == e2.Sender) is null)
                    .Select(e =>
                    {
                        var command = this.connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = $"DELETE FROM {this.options.TableName} WHERE Sender = $sender";
                        command.Parameters.AddWithValue("$sender", e.Sender);
                        return command;
                    }));
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
                            e.District != existingEntry.District ||
                            e.DistrictLanguage != (int)(existingEntry?.DistrictLanguage ?? DistrictLanguage.English) ||
                            e.DistrictNumber != existingEntry?.DistrictNumber;
                })
                .Select(e =>
                {
                    var command = this.connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = $@"
                        INSERT INTO {this.options.TableName} (Sender, DistrictNumber, DistrictLanguage, District, MapId, PartyId, Message, PartySize, HeroCount, HardMode, SearchType, PrimaryId, SecondaryId, Level, Timestamp)
                        VALUES ($sender, $districtNumber, $districtLanguage, $district, $mapId, $partyId, $message, $partySize, $heroCount, $hardMode, $searchType, $primary, $secondary, $level, $timestamp)
                        ON CONFLICT(Sender) DO UPDATE SET
                            DistrictNumber = excluded.DistrictNumber,
                            DistrictLanguage = excluded.DistrictLanguage,
                            District = excluded.District,
                            MapId = excluded.MapId,
                            PartyId = excluded.PartyId,
                            Message = excluded.Message,
                            PartySize = excluded.PartySize,
                            HeroCount = excluded.HeroCount,
                            HardMode = excluded.HardMode,
                            SearchType = excluded.SearchType,
                            PrimaryId = excluded.PrimaryId,
                            SecondaryId = excluded.SecondaryId,
                            Level = excluded.Level,
                            Timestamp = excluded.Timestamp;
                    ";

                    command.Parameters.AddWithValue("$sender", e.Sender);
                    command.Parameters.AddWithValue("$districtNumber", e.DistrictNumber);
                    command.Parameters.AddWithValue("$districtLanguage", e.DistrictLanguage);
                    command.Parameters.AddWithValue("$district", e.District);
                    command.Parameters.AddWithValue("$mapId", e.MapId);
                    command.Parameters.AddWithValue("$partyId", e.PartyId);
                    command.Parameters.AddWithValue("$message", e.Message?.As<object>() ?? DBNull.Value);
                    command.Parameters.AddWithValue("$partySize", e.PartySize);
                    command.Parameters.AddWithValue("$heroCount", e.HeroCount);
                    command.Parameters.AddWithValue("$hardMode", e.HardMode);
                    command.Parameters.AddWithValue("$searchType", e.SearchType);
                    command.Parameters.AddWithValue("$primary", e.Primary);
                    command.Parameters.AddWithValue("$secondary", e.Secondary);
                    command.Parameters.AddWithValue("$level", e.Level);
                    command.Parameters.AddWithValue("$timestamp", e.Timestamp.HasValue ? e.Timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ") : DBNull.Value);
                    return command;
                }));
            if (actions.None())
            {
                scopedLogger.LogInformation("No change detected. Skipping operation");
                return true;
            }

            var results = new List<int>();
            foreach(var command in actions)
            {
                var result = await command.ExecuteNonQueryAsync(cancellationToken);
                scopedLogger.LogInformation($"Database operation returned {result}");
                results.Add(result);
            }

            await transaction.CommitAsync(cancellationToken);
            actions.ForEach(command => command.Dispose());
            // Refresh local cache
            await this.allPartiesCache.ForceCacheRefresh();
            return results.None(r => r == 0);
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while setting party searches");
            return false;
        }
    }

    public async Task<List<PartySearchEntry>?> GetPartySearches(Map map, int district, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPartySearches), string.Empty);
        try
        {
            return (await this.allPartiesCache.GetValue())
                .Where(p => map.Id == p.Map?.Id && district == p.District)
                .SelectMany(p => p.PartySearchEntries ?? [])
                .ToList();
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception");
        }

        return default;
    }

    private async Task<List<Server.Models.PartySearch>> GetAllPartySearchesInternal()
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetAllPartySearchesInternal), string.Empty);
        try
        {
            scopedLogger.LogInformation("Refreshing party search cache");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            return await this.GetSearchesInternal(cts.Token);
        }
        catch (Exception ex)
        {
            scopedLogger.LogError(ex, "Failed to refresh cache");
            throw;
        }
    }

    private async Task<List<Server.Models.PartySearch>> GetSearchesInternal(CancellationToken cancellationToken)
    {
        var entries = new List<Server.Models.PartySearch>();
        using var command = this.connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {this.options.TableName}";
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            _ = Map.TryParse(reader.GetInt32(reader.GetOrdinal("MapId")), out var map);
            _ = Profession.TryParse(reader.GetInt32(reader.GetOrdinal("PrimaryId")), out var primary);
            _ = Profession.TryParse(reader.GetInt32(reader.GetOrdinal("SecondaryId")), out var secondary);
            var entry = new PartySearchEntry
            {
                Sender = reader.GetString(reader.GetOrdinal("Sender")),
                DistrictNumber = reader.GetInt32(reader.GetOrdinal("DistrictNumber")),
                DistrictLanguage = reader.GetInt32(reader.GetOrdinal("DistrictLanguage")).Cast<DistrictLanguage>(),
                District = reader.GetInt32(reader.GetOrdinal("District")),
                PartyId = reader.GetInt32(reader.GetOrdinal("PartyId")),
                Message = reader.IsDBNull(reader.GetOrdinal("Message")) ? null : reader.GetString(reader.GetOrdinal("Message")),
                PartySize = reader.GetInt32(reader.GetOrdinal("PartySize")),
                HeroCount = reader.GetInt32(reader.GetOrdinal("HeroCount")),
                HardMode = reader.GetInt32(reader.GetOrdinal("HardMode")).Cast<HardModeState>(),
                SearchType = reader.GetInt32(reader.GetOrdinal("SearchType")),
                Primary = primary,
                Secondary = secondary,
                Level = reader.GetInt32(reader.GetOrdinal("Level"))
            };

            var maybePartySearch = entries.FirstOrDefault(p => p.Map == map && p.District == entry.District);
            if (maybePartySearch is null)
            {
                maybePartySearch = new Server.Models.PartySearch
                {
                    District = entry.District,
                    Map = map,
                    PartySearchEntries = []
                };

                entries.Add(maybePartySearch);
            }

            maybePartySearch.PartySearchEntries?.Add(entry);
        }

        return entries;
    }

    private static void EnsureTableExists(SqliteConnection connection, string tableName)
    {
        TableSemaphore.Wait();
        using var command = connection.CreateCommand();
        command.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    Sender TEXT PRIMARY KEY NOT NULL,
                    DistrictNumber INTEGER NOT NULL,
                    DistrictLanguage INTEGER NOT NULL,
                    District INTEGER NOT NULL,
                    MapId INTEGER NOT NULL,
                    PartyId INTEGER NOT NULL,
                    Message TEXT,
                    PartySize INTEGER NOT NULL,
                    HeroCount INTEGER NOT NULL,
                    HardMode INTEGER NOT NULL,
                    SearchType INTEGER NOT NULL,
                    PrimaryId INTEGER NOT NULL,
                    SecondaryId INTEGER NOT NULL,
                    Level INTEGER NOT NULL,
                    Timestamp DATETIME
                );
            ";
        command.ExecuteNonQuery();
        TableSemaphore.Release();
    }
}

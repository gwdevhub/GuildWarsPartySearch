using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.BotStatus.Models;
using GuildWarsPartySearch.Server.Services.Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class BotHistorySqliteDatabase : IBotHistoryDatabase
{
    private static readonly SemaphoreSlim TableSemaphore = new(1);

    private readonly SqliteConnection connection;
    private readonly BotHistoryDatabaseOptions options;
    private readonly ILogger<BotHistorySqliteDatabase> logger;

    public BotHistorySqliteDatabase(
        SqliteConnection connection,
        IOptions<BotHistoryDatabaseOptions> options,
        ILogger<BotHistorySqliteDatabase> logger)
    {
        this.connection = connection.ThrowIfNull();
        this.options = options.ThrowIfNull().Value.ThrowIfNull();
        this.logger = logger.ThrowIfNull();

        EnsureTableExists(this.connection, this.options.TableName);
    }

    public async Task<bool> RecordBotActivity(Bot bot, BotActivity.ActivityType activityType, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.RecordBotActivity), bot.Name);
        var activity = new BotActivity
        {
            Activity = activityType,
            MapId = bot.Map.Id,
            Name = bot.Name,
            TimeStamp = DateTime.Now
        };

        if (!await this.InsertActivity(activity, cancellationToken))
        {
            scopedLogger.LogError($"Failed to record activity {activityType}");
            return false;
        }

        return true;
    }

    public async Task<IEnumerable<BotActivity>> GetBotActivity(string botName, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetAllBotsActivity), botName);
        try
        {
            return await this.GetBotActivityInternal(botName, cancellationToken);
        }
        catch (Exception ex)
        {
            scopedLogger.LogError(ex, "Encountered exception");
            throw;
        }
    }

    public async Task<IEnumerable<BotActivity>> GetBotActivity(Bot bot, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetAllBotsActivity), bot.Name);
        try
        {
            return await this.GetBotActivityInternal(bot.Name, cancellationToken);
        }
        catch (Exception ex)
        {
            scopedLogger.LogError(ex, "Encountered exception");
            throw;
        }
    }

    public async Task<IEnumerable<BotActivity>> GetBotsActivityOnMap(Map map, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetAllBotsActivity), map.Id.ToString());
        try
        {
            return await this.GetBotsActivityOnMapInternal(map.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            scopedLogger.LogError(ex, "Encountered exception");
            throw;
        }
    }

    public async Task<IEnumerable<BotActivity>> GetAllBotsActivity(CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetAllBotsActivity), string.Empty);
        try
        {
            return await this.GetAllBotsActivityInternal(cancellationToken);
        }
        catch(Exception ex)
        {
            scopedLogger.LogError(ex, "Encountered exception");
            throw;
        }
    }

    private async Task<IEnumerable<BotActivity>> GetAllBotsActivityInternal(CancellationToken cancellationToken)
    {
        using var command = this.connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {this.options.TableName}";
        return await GetActivityInternal(command, cancellationToken);
    }

    private async Task<IEnumerable<BotActivity>> GetBotsActivityOnMapInternal(int mapId, CancellationToken cancellationToken)
    {
        using var command = this.connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {this.options.TableName} WHERE MapId = $mapId";
        command.Parameters.AddWithValue("mapId", mapId);
        return await GetActivityInternal(command, cancellationToken);
    }

    private async Task<IEnumerable<BotActivity>> GetBotActivityInternal(string name, CancellationToken cancellationToken)
    {
        using var command = this.connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {this.options.TableName} WHERE Name = $name";
        command.Parameters.AddWithValue("name", name);
        return await GetActivityInternal(command, cancellationToken);
    }

    private async Task<bool> InsertActivity(BotActivity activity, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.InsertActivity), activity.Name);
        try
        {
            using var command = this.connection.CreateCommand();
            command.CommandText = $@"
                        INSERT INTO {this.options.TableName} (Name, MapId, ActivityType, TimeStamp)
                        VALUES ($name, $mapId, $activity, $timestamp)
                    ";
            command.Parameters.AddWithValue("name", activity.Name);
            command.Parameters.AddWithValue("mapId", activity.MapId);
            command.Parameters.AddWithValue("activity", (int)activity.Activity);
            command.Parameters.AddWithValue("timestamp", activity.TimeStamp);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            scopedLogger.LogInformation($"Logging bot activity. Result {result}");
            return result == 1;
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while setting party searches");
            return false;
        }
    }

    private static async Task<IEnumerable<BotActivity>> GetActivityInternal(SqliteCommand command, CancellationToken cancellationToken)
    {
        var activities = new List<BotActivity>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var activity = new BotActivity
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                MapId = reader.GetInt32(reader.GetOrdinal("MapId")),
                Activity = reader.GetInt32(reader.GetOrdinal("ActivityType")).Cast<BotActivity.ActivityType>(),
                TimeStamp = reader.GetDateTime(reader.GetOrdinal("TimeStamp"))
            };
            activities.Add(activity);
        }

        return activities;
    }

    private static void EnsureTableExists(SqliteConnection connection, string tableName)
    {
        TableSemaphore.Wait();
        using var command = connection.CreateCommand();
        command.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    MapId INTEGER NOT NULL,
                    ActivityType INTEGER NOT NULL,
                    TimeStamp DATETIME
                );
            ";
        command.ExecuteNonQuery();
        TableSemaphore.Release();
    }
}

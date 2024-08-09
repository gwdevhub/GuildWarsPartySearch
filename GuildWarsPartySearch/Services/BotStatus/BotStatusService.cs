using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.BotStatus.Models;
using GuildWarsPartySearch.Server.Services.Database;
using GuildWarsPartySearch.Server.Services.Feed;
using System.Collections.Concurrent;
using System.Core.Extensions;
using System.Extensions;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus;

public sealed class BotStatusService : IBotStatusService
{
    private readonly ConcurrentDictionary<string, Bot> connectedBots = [];

    private readonly ILiveFeedService liveFeedService;
    private readonly IBotHistoryDatabase database;
    private readonly ILogger<BotStatusService> logger;

    public BotStatusService(
        ILiveFeedService liveFeedService,
        IBotHistoryDatabase botHistoryDatabase,
        IHostApplicationLifetime lifetime,
        ILogger<BotStatusService> logger)
    {
        this.liveFeedService = liveFeedService.ThrowIfNull();
        this.database = botHistoryDatabase.ThrowIfNull();
        this.logger = logger.ThrowIfNull();

        lifetime.ThrowIfNull().ApplicationStopping.Register(this.ShutDownConnections);
    }

    public async Task<bool> AddBot(string botId, WebSocket client, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.AddBot), botId);
        if (botId.IsNullOrWhiteSpace())
        {
            scopedLogger.LogInformation("Unable to add bot. Null id");
            return false;
        }

        var tokens = botId.Split('-');
        if (tokens.Length != 3)
        {
            scopedLogger.LogInformation($"Unable to add bot. Malformed id");
            return false;
        }

        if (!int.TryParse(tokens[1], out var mapId) ||
            !Map.TryParse(mapId, out var map))
        {
            scopedLogger.LogInformation($"Unable to add bot. Malformed mapid");
            return false;
        }

        if (!int.TryParse(tokens[2], out var district))
        {
            scopedLogger.LogInformation($"Unable to add bot. Malformed district");
            return false;
        }

        var bot = new Bot { Name = tokens[0], Map = map, District = district, WebSocket = client };
        if (!this.connectedBots.TryAdd(botId, bot))
        {
            scopedLogger.LogInformation("Unable to add bot. Failed to add to cache");
            return false;
        }

        await this.database.RecordBotActivity(bot, Database.Models.BotActivity.ActivityType.Connect, cancellationToken);
        scopedLogger.LogDebug($"Added bot {botId}");
        return true;
    }

    public async Task<IEnumerable<BotActivityResponse>> GetActivitiesForBot(string botName, CancellationToken cancellationToken)
    {
        return (await this.database.GetBotActivity(botName, cancellationToken))
            .Select(a =>
            {
                _ = Map.TryParse(a.MapId, out var map);
                return new BotActivityResponse
                {
                    Name = a.Name,
                    Map = map,
                    Activity = Enum.GetName(a.Activity),
                    TimeStamp = a.TimeStamp,
                };
            });
    }

    public async Task<IEnumerable<BotActivityResponse>> GetActivitiesForMap(int mapId, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetActivitiesForMap), mapId.ToString());
        if (!Map.TryParse(mapId, out var map))
        {
            scopedLogger.LogError("Failed to parse map id");
            return [];
        }

        return (await this.database.GetBotsActivityOnMap(map, cancellationToken))
            .Select(a =>
            {
                _ = Map.TryParse(a.MapId, out var map);
                return new BotActivityResponse
                {
                    Name = a.Name,
                    Map = map,
                    Activity = Enum.GetName(a.Activity),
                    TimeStamp = a.TimeStamp,
                };
            });
    }

    public async Task<IEnumerable<BotActivityResponse>> GetActivitiesForMap(string mapName, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetActivitiesForMap), mapName);
        if (!Map.TryParse(mapName, out var map))
        {
            scopedLogger.LogError("Failed to parse map id");
            return [];
        }

        return (await this.database.GetBotsActivityOnMap(map, cancellationToken))
            .Select(a =>
            {
                _ = Map.TryParse(a.MapId, out var map);
                return new BotActivityResponse
                {
                    Name = a.Name,
                    Map = map,
                    Activity = Enum.GetName(a.Activity),
                    TimeStamp = a.TimeStamp,
                };
            });
    }

    public async Task<IEnumerable<BotActivityResponse>> GetAllActivities(CancellationToken cancellationToken)
    {
        return (await this.database.GetAllBotsActivity(cancellationToken))
            .Select(a =>
            {
                _ = Map.TryParse(a.MapId, out var map);
                return new BotActivityResponse
                {
                    Name = a.Name,
                    Map = map,
                    Activity = Enum.GetName(a.Activity),
                    TimeStamp = a.TimeStamp,
                };
            });
    }

    public Task<IEnumerable<Models.BotStatus>> GetBots(CancellationToken cancellationToken)
    {
        return Task.FromResult(this.connectedBots.Select(b => new Models.BotStatus { Id = b.Key, Name = b.Value.Name, District = b.Value.District, Map = b.Value.Map, LastSeen = b.Value.LastSeen }));
    }

    public async Task<bool> RecordBotUpdateActivity(string botId, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.RecordBotUpdateActivity), botId);
        if (!this.connectedBots.TryGetValue(botId, out var bot))
        {
            scopedLogger.LogError("Failed to find bot by id");
            return false;
        }

        bot.LastSeen = DateTime.Now;
        await this.database.RecordBotActivity(bot, Database.Models.BotActivity.ActivityType.Update, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveBot(string botId, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.RemoveBot), botId);
        if (botId.IsNullOrEmpty())
        {
            scopedLogger.LogInformation("Unable to remove bot. Null id");
            return false;
        }

        if (!this.connectedBots.TryRemove(botId, out var bot) || 
            bot is null)
        {
            scopedLogger.LogInformation("Unable to remove bot. Failed to remove bot from cache");
            return false;
        }

        await this.database.RecordBotActivity(bot, Database.Models.BotActivity.ActivityType.Disconnect, cancellationToken);
        await this.liveFeedService.PushUpdate(new Server.Models.PartySearch
        {
            District = bot.District,
            Map = bot.Map,
            PartySearchEntries = []
        }, cancellationToken);
        scopedLogger.LogDebug("Removed bot");
        return true;
    }

    private void ShutDownConnections()
    {
        foreach (var bot in this.connectedBots)
        {
            bot.Value.WebSocket.Abort();
        }
    }
}

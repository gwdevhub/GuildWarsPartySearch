using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Services.BotStatus.Models;
using System.Collections.Concurrent;
using System.Core.Extensions;
using System.Extensions;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus;

public sealed class BotStatusService : IBotStatusService
{
    private readonly ConcurrentDictionary<string, Bot> connectedBots = [];

    private readonly ILogger<BotStatusService> logger;

    public BotStatusService(
        IHostApplicationLifetime lifetime,
        ILogger<BotStatusService> logger)
    {
        lifetime.ApplicationStopping.Register(this.ShutDownConnections);
        this.logger = logger.ThrowIfNull();
    }

    public Task<bool> AddBot(string botId, WebSocket client)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.AddBot), botId);
        if (botId.IsNullOrWhiteSpace())
        {
            scopedLogger.LogInformation("Unable to add bot. Null id");
            return Task.FromResult(false);
        }

        var tokens = botId.Split('-');
        if (tokens.Length != 3)
        {
            scopedLogger.LogInformation($"Unable to add bot. Malformed id");
            return Task.FromResult(false);
        }

        if (!int.TryParse(tokens[1], out var mapId) ||
            !Map.TryParse(mapId, out var map))
        {
            scopedLogger.LogInformation($"Unable to add bot. Malformed mapid");
            return Task.FromResult(false);
        }

        if (!int.TryParse(tokens[2], out var district))
        {
            scopedLogger.LogInformation($"Unable to add bot. Malformed district");
            return Task.FromResult(false);
        }

        if (!this.connectedBots.TryAdd(botId, new Bot { Name = tokens[0], Map = map, District = district, WebSocket = client }))
        {
            scopedLogger.LogInformation("Unable to add bot. Failed to add to cache");
            return Task.FromResult(false);
        }

        scopedLogger.LogDebug($"Added bot {botId}");
        return Task.FromResult(true);
    }

    public Task<IEnumerable<Models.BotStatus>> GetBots()
    {
        return Task.FromResult(this.connectedBots.Select(b => new Models.BotStatus { Id = b.Key, Name = b.Value.Name, District = b.Value.District, Map = b.Value.Map, LastSeen = b.Value.LastSeen }));
    }

    public Task<bool> RecordBotActivity(string botId)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.RecordBotActivity), botId);
        if (!this.connectedBots.TryGetValue(botId, out var bot))
        {
            scopedLogger.LogError("Failed to find bot by id");
            return Task.FromResult(false);
        }

        bot.LastSeen = DateTime.Now;
        return Task.FromResult(true);
    }

    public Task<bool> RemoveBot(string botId)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.RemoveBot), botId);
        if (botId.IsNullOrEmpty())
        {
            scopedLogger.LogInformation("Unable to remove bot. Null id");
            return Task.FromResult(false);
        }

        if (!this.connectedBots.TryRemove(botId, out var client) || 
            client is null)
        {
            scopedLogger.LogInformation("Unable to remove bot. Failed to remove bot from cache");
            return Task.FromResult(false);
        }

        scopedLogger.LogDebug("Removed bot");
        return Task.FromResult(true);
    }

    private void ShutDownConnections()
    {
        foreach (var bot in this.connectedBots)
        {
            bot.Value.WebSocket.Abort();
        }
    }
}

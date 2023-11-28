using System.Collections.Concurrent;
using System.Core.Extensions;
using System.Extensions;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus;

public sealed class BotStatusService : IBotStatusService
{
    private readonly ConcurrentDictionary<string, WebSocket> connectedBots = [];

    private readonly ILogger<BotStatusService> logger;

    public BotStatusService(
        ILogger<BotStatusService> logger)
    {
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

        if (!this.connectedBots.TryAdd(botId, client))
        {
            scopedLogger.LogInformation("Unable to add bot. Failed to add to cache");
            return Task.FromResult(false);
        }

        scopedLogger.LogDebug("Added bot");
        return Task.FromResult(true);
    }

    public Task<IEnumerable<string>> GetBots()
    {
        var bots = this.connectedBots.Keys.AsEnumerable();
        return Task.FromResult(bots);
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
}

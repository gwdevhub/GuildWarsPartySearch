using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus;

public interface IBotStatusService
{
    Task<bool> RecordBotActivity(string botId);
    Task<bool> AddBot(string botId, WebSocket client);
    Task<bool> RemoveBot(string botId);
    Task<IEnumerable<Models.BotStatus>> GetBots();
}

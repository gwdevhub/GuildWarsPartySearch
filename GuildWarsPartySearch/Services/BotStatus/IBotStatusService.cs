using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus;

public interface IBotStatusService
{
    Task<bool> AddBot(string botId, WebSocket client);
    Task<bool> RemoveBot(string botId);
    Task<IEnumerable<string>> GetBots();
}

using GuildWarsPartySearch.Server.Models.Endpoints;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus;

public interface IBotStatusService
{
    Task<bool> RecordBotUpdateActivity(string botId, CancellationToken cancellationToken);
    Task<bool> AddBot(string botId, WebSocket client, CancellationToken cancellationToken);
    Task<bool> RemoveBot(string botId, CancellationToken cancellationToken);
    Task<IEnumerable<BotActivityResponse>> GetAllActivities(CancellationToken cancellationToken);
    Task<IEnumerable<BotActivityResponse>> GetActivitiesForBot(string botName, CancellationToken cancellationToken);
    Task<IEnumerable<BotActivityResponse>> GetActivitiesForMap(int mapId, CancellationToken cancellationToken);
    Task<IEnumerable<BotActivityResponse>> GetActivitiesForMap(string mapName, CancellationToken cancellationToken);
    Task<IEnumerable<Models.BotStatus>> GetBots(CancellationToken cancellationToken);
}

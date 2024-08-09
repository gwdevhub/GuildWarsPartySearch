using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Services.BotStatus.Models;
using GuildWarsPartySearch.Server.Services.Database.Models;

namespace GuildWarsPartySearch.Server.Services.Database;

public interface IBotHistoryDatabase
{
    Task<bool> RecordBotActivity(Bot bot, BotActivity.ActivityType activityType, CancellationToken cancellationToken);
    Task<IEnumerable<BotActivity>> GetBotActivity(string botName, CancellationToken cancellationToken);
    Task<IEnumerable<BotActivity>> GetBotActivity(Bot bot, CancellationToken cancellationToken);
    Task<IEnumerable<BotActivity>> GetBotsActivityOnMap(Map map, CancellationToken cancellationToken);
    Task<IEnumerable<BotActivity>> GetAllBotsActivity(CancellationToken cancellationToken);
}

using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus.Models;

internal class Bot
{
    public string Name { get; init; } = default!;
    public Map Map { get; init; } = default!;
    public int District { get; init; } = default!;
    public WebSocket WebSocket { get; init; } = default!;
    public DateTime LastSeen { get; set; } = DateTime.Now;
}

using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.BotStatus.Models;

public sealed class Bot
{
    public string Name { get; init; } = default!;
    public Map Map { get; set; } = Map.None;
    public int? District { get; set; } = default!;
    public WebSocket WebSocket { get; init; } = default!;
    public DateTime LastSeen { get; set; } = DateTime.Now;
}

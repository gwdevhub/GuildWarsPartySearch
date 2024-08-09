using GuildWarsPartySearch.Common.Models.GuildWars;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

public sealed class BotActivityResponse
{
    public string? Name { get; set; }
    public Map? Map { get; set; }
    public string? Activity {  get; set; }
    public DateTime? TimeStamp { get; set; }
}

using GuildWarsPartySearch.Common.Models.GuildWars;
using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Services.BotStatus.Models;

public class BotStatus
{
    [JsonProperty(nameof(Id))]
    public string? Id { get; set; }
    [JsonProperty(nameof(Name))]
    public string? Name { get; set; }
    [JsonProperty(nameof(Map))]
    public Map? Map { get; set; }
    [JsonProperty(nameof(District))]
    public int District { get; set; }
    [JsonProperty(nameof(LastSeen))]
    public DateTime? LastSeen { get; set; }
}

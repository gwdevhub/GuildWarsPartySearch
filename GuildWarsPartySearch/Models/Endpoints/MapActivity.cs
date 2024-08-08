using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

public class MapActivity
{
    [JsonProperty(nameof(MapId))]
    public int MapId { get; set; }
    [JsonProperty(nameof(District))]
    public int District {  get; set; }
    [JsonProperty(nameof(LastUpdate))]
    public DateTime LastUpdate { get; set; }
}

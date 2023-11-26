using GuildWarsPartySearch.Common.Models.GuildWars;
using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

public sealed class PostPartySearchRequest
{
    [JsonProperty(nameof(Campaign))]
    public Campaign? Campaign { get; set; }

    [JsonProperty(nameof(Continent))]
    public Continent? Continent { get; set; }

    [JsonProperty(nameof(Region))]
    public Region? Region { get; set; }

    [JsonProperty(nameof(Map))]
    public Map? Map { get; set; }

    [JsonProperty(nameof(District))]
    public string? District { get; set; }

    [JsonProperty(nameof(PartySearchEntries))]
    public List<PartySearchEntry>? PartySearchEntries { get; set; }
}

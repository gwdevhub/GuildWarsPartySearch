using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models;

public sealed class PartySearch
{
    [JsonPropertyName("map_id")]
    public Map? Map { get; set; }

    [JsonPropertyName("district")]
    public int District { get; set; }

    [JsonPropertyName("parties")]
    public List<PartySearchEntry>? PartySearchEntries { get; set; }
}

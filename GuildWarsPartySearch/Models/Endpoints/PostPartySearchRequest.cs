using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebSocketConverter<JsonWebSocketMessageConverter<PostPartySearchRequest>, PostPartySearchRequest>]
public sealed class PostPartySearchRequest
{
    [JsonPropertyName("map_id")]
    public Map? Map { get; set; }

    [JsonPropertyName("district_region")]
    public DistrictRegion? DistrictRegion { get; set; }

    [JsonPropertyName("district_number")]
    public int DistrictNumber { get; set; }

    [JsonPropertyName("district_language")]
    public DistrictLanguage? DistrictLanguage { get; set; }

    [JsonPropertyName("parties")]
    public List<PartySearchEntry>? PartySearchEntries { get; set; }
}

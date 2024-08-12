using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebSocketConverter<JsonWebSocketMessageConverter<PostPartySearchRequest>, PostPartySearchRequest>]
public sealed class PostPartySearchRequest
{
    [JsonPropertyName("full_list")]
    public bool GetFullList { get; set; }

    [JsonPropertyName("map_id")]
    public Map? Map { get; set; }

    [JsonPropertyName("district")]
    public int? District { get; set; }

    [JsonPropertyName("parties")]
    public List<PartySearchEntry>? PartySearchEntries { get; set; }
}

using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebSocketConverter<JsonWebSocketMessageConverter<LiveFeedRequest>, LiveFeedRequest>]
public sealed class LiveFeedRequest
{
    [JsonPropertyName("full_list")]
    public bool GetFullList { get; set; }
}

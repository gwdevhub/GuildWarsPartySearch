using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;
using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebSocketConverter<JsonWebSocketMessageConverter<PostPartySearchResponse>, PostPartySearchResponse>]
public sealed class PostPartySearchResponse
{
    [JsonProperty(nameof(Result))]
    public int Result { get; set; }

    [JsonProperty(nameof(Description))]
    public string? Description { get; set; }
}

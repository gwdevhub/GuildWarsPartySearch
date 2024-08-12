using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebSocketConverter<JsonWebSocketMessageConverter<PostPartySearchResponse>, PostPartySearchResponse>]
public sealed class PostPartySearchResponse
{
    [JsonPropertyName(nameof(Result))]
    public int Result { get; set; }

    [JsonPropertyName(nameof(Description))]
    public string? Description { get; set; }

    [JsonPropertyName(nameof(PartySearches))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<PartySearch>? PartySearches { get; set; }
}

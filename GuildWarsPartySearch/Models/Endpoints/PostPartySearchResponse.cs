using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

public sealed class PostPartySearchResponse
{
    [JsonProperty(nameof(Result))]
    public int Result { get; set; }

    [JsonProperty(nameof(Description))]
    public string? Description { get; set; }
}

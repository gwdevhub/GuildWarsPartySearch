using GuildWarsPartySearch.Server.Converters;
using MTSC.Common.WebSockets.RoutingModules;
using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebsocketMessageConvert(typeof(PostPartyResponseWebsocketMessageConverter))]
public sealed class PostPartySearchResponse
{
    [JsonProperty(nameof(Result))]
    public int Result { get; set; }

    [JsonProperty(nameof(Description))]
    public string? Description { get; set; }
}

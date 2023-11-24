using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Options;

public sealed class ServerOptions
{
    [JsonProperty(nameof(ApiKey))]
    public string? ApiKey { get; set; }
}

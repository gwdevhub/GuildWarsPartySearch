using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Options;

public sealed class ServerOptions
{
    [JsonProperty(nameof(ApiKey))]
    public string? ApiKey { get; set; }

    [JsonProperty(nameof(InactivityTimeout))]
    public TimeSpan? InactivityTimeout { get; set; }

    [JsonProperty(nameof(HeartbeatFrequency))]
    public TimeSpan? HeartbeatFrequency { get; set; }
}

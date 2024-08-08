using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class ServerOptions
{
    [JsonPropertyName(nameof(InactivityTimeout))]
    public TimeSpan? InactivityTimeout { get; set; }

    [JsonPropertyName(nameof(HeartbeatFrequency))]
    public TimeSpan? HeartbeatFrequency { get; set; }

    [JsonPropertyName(nameof(Port))]
    public int? Port { get; set; }
}

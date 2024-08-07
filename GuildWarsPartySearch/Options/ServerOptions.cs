using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class ServerOptions
{
    [JsonPropertyName(nameof(Certificate))]
    public string? Certificate { get; set; }

    [JsonPropertyName(nameof(CertificatePassword))]
    public string? CertificatePassword { get; set; }

    [JsonPropertyName(nameof(ApiKey))]
    public string? ApiKey { get; set; }

    [JsonPropertyName(nameof(InactivityTimeout))]
    public TimeSpan? InactivityTimeout { get; set; }

    [JsonPropertyName(nameof(HeartbeatFrequency))]
    public TimeSpan? HeartbeatFrequency { get; set; }
}

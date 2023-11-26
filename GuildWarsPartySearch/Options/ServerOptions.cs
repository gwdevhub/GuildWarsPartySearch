using GuildWarsPartySearch.Server.Converters;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace GuildWarsPartySearch.Server.Options;

public sealed class ServerOptions
{
    [JsonProperty(nameof(Certificate))]
    [JsonConverter(typeof(Base64ToCertificateConverter))]
    public X509Certificate2? Certificate { get; set; }

    [JsonProperty(nameof(ApiKey))]
    public string? ApiKey { get; set; }

    [JsonProperty(nameof(InactivityTimeout))]
    public TimeSpan? InactivityTimeout { get; set; }

    [JsonProperty(nameof(HeartbeatFrequency))]
    public TimeSpan? HeartbeatFrequency { get; set; }
}

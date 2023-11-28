using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class TelemetryOptions
{
    [JsonPropertyName(nameof(SuccessfulStatusCodes))]
    public List<int>? SuccessfulStatusCodes { get; set; }
}

using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class EnvironmentOptions
{
    [JsonPropertyName(nameof(Name))]
    public string? Name { get; init; }
}

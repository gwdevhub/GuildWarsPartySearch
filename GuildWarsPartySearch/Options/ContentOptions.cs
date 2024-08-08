using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class ContentOptions
{
    [JsonPropertyName(nameof(StagingFolder))]
    public string StagingFolder { get; set; } = "Content";
}

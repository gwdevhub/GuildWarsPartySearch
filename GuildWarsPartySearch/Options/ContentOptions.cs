using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class ContentOptions : IAzureBlobStorageOptions
{
    [JsonPropertyName(nameof(UpdateFrequency))]
    public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromMinutes(5);

    [JsonPropertyName(nameof(StagingFolder))]
    public string StagingFolder { get; set; } = "Content";

    [JsonPropertyName(nameof(ContainerName))]
    public string ContainerName { get; set; } = default!;
}

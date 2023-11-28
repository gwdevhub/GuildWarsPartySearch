namespace GuildWarsPartySearch.Server.Options;

public sealed class ContentOptions : IAzureBlobStorageOptions
{
    public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromMinutes(5);
    public string StagingFolder { get; set; } = "Content";
    public string ContainerName { get; set; } = default!;
}

namespace GuildWarsPartySearch.Server.Options;

public sealed class ContentOptions
{
    public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromMinutes(5);
    public string StagingFolder { get; set; } = "Content";
}

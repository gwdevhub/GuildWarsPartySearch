using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public class SQLiteDatabaseOptions
{
    [JsonPropertyName(nameof(Path))]
    public string Path { get; set; } = string.Empty;
}

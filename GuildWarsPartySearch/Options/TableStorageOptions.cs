using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Options;

public sealed class TableStorageOptions
{
    [JsonProperty(nameof(TableName))]
    public string? TableName { get; set; }

    [JsonProperty(nameof(ConnectionString))]
    public string? ConnectionString { get; set; }
}

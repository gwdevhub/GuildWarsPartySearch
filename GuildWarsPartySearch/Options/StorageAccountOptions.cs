using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Options;

public sealed class StorageAccountOptions
{
    [JsonProperty(nameof(TableName))]
    public string? TableName { get; set; }

    [JsonProperty(nameof(ConnectionString))]
    public string? ConnectionString { get; set; }

    [JsonProperty(nameof(ContainerName))]
    public string? ContainerName { get; set; }
}

using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class StorageAccountOptions
{
    [JsonPropertyName(nameof(TableName))]
    public string? TableName { get; set; }

    [JsonPropertyName(nameof(ConnectionString))]
    public string? ConnectionString { get; set; }

    [JsonPropertyName(nameof(ContainerName))]
    public string? ContainerName { get; set; }
}

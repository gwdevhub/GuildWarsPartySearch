using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class StorageAccountOptions
{
    [JsonPropertyName(nameof(Name))]
    public string? Name { get; set; }
}

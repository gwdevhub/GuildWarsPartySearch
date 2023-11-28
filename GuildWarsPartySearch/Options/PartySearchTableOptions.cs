using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public class PartySearchTableOptions : IAzureTableStorageOptions
{
    [JsonPropertyName(nameof(TableName))]
    public string TableName { get; set; } = default!;
}

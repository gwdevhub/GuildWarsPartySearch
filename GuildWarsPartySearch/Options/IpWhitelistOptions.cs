using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public class IpWhitelistOptions
{
    [JsonPropertyName(nameof(Addresses))]
    public List<string> Addresses { get; set; } = [];
}

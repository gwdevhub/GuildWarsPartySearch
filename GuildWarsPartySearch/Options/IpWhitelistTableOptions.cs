using GuildWarsPartySearch.Server.Options.Azure;

namespace GuildWarsPartySearch.Server.Options;

public class IpWhitelistTableOptions : IAzureTableStorageOptions
{
    public string TableName { get; set; } = default!;
}

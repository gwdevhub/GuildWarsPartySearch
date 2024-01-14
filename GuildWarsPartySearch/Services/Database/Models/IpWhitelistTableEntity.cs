using Azure;
using Azure.Data.Tables;

namespace GuildWarsPartySearch.Server.Services.Database.Models;

public class IpWhitelistTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Whitelist";
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

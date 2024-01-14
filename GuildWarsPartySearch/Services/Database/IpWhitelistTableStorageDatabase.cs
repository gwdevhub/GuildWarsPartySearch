
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Azure;
using GuildWarsPartySearch.Server.Services.Database.Models;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class IpWhitelistTableStorageDatabase : IIpWhitelistDatabase
{
    private readonly NamedTableClient<IpWhitelistTableOptions> client;
    private readonly ILogger<IpWhitelistTableStorageDatabase> logger;

    private List<IpWhitelistTableEntity>? whitelistCache;
    private DateTime? lastCacheTime;

    public IpWhitelistTableStorageDatabase(
        NamedTableClient<IpWhitelistTableOptions> client,
        ILogger<IpWhitelistTableStorageDatabase> logger)
    {
        this.client = client.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public async Task<IEnumerable<string>> GetWhitelistedAddresses(CancellationToken cancellationToken)
    {
        if (this.whitelistCache is not null &&
            this.lastCacheTime.HasValue &&
            DateTime.UtcNow - this.lastCacheTime.Value < TimeSpan.FromSeconds(15))
        {
            return this.whitelistCache.Select(ip => ip.RowKey);
        }

        this.whitelistCache = [];
        var entries = this.client.QueryAsync<IpWhitelistTableEntity>("PartitionKey eq 'Whitelist'", cancellationToken: cancellationToken);
        await foreach(var entry in entries)
        {
            this.whitelistCache.Add(entry);
        }

        this.lastCacheTime = DateTime.UtcNow;
        return this.whitelistCache.Select(ip => ip.RowKey);
    }
}

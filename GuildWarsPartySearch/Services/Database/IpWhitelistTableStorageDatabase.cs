
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Azure;
using GuildWarsPartySearch.Server.Services.Database.Models;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class IpWhitelistTableStorageDatabase : IIpWhitelistDatabase
{
    private readonly NamedTableClient<IpWhitelistTableOptions> client;
    private readonly IpWhitelistTableOptions options;
    private readonly ILogger<IpWhitelistTableStorageDatabase> logger;
    private readonly ValueCache<List<IpWhitelistTableEntity>> whitelistCache;

    public IpWhitelistTableStorageDatabase(
        NamedTableClient<IpWhitelistTableOptions> client,
        IOptions<IpWhitelistTableOptions> options,
        ILogger<IpWhitelistTableStorageDatabase> logger)
    {
        this.client = client.ThrowIfNull();
        this.options = options.ThrowIfNull().Value.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
        this.whitelistCache = new ValueCache<List<IpWhitelistTableEntity>>(this.GetWhitelistedAddressList, this.options.CacheDuration);
    }

    public async Task<IEnumerable<string>> GetWhitelistedAddresses(CancellationToken cancellationToken)
    {
        return (await this.whitelistCache.GetValue()).Select(ip => ip.RowKey);
    }

    private async Task<List<IpWhitelistTableEntity>> GetWhitelistedAddressList()
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetWhitelistedAddressList), string.Empty);
        try
        {
            scopedLogger.LogInformation("Ip whitelist cache expired. Refreshing cache");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var whitelistCache = new List<IpWhitelistTableEntity>();
            var entries = this.client.QueryAsync<IpWhitelistTableEntity>("PartitionKey eq 'Whitelist'", cancellationToken: cts.Token);
            await foreach (var entry in entries)
            {
                whitelistCache.Add(entry);
            }

            return whitelistCache;
        }
        catch (Exception ex)
        {
            scopedLogger.LogError(ex, "Failed to refresh cache");
            throw;
        }
    }
}

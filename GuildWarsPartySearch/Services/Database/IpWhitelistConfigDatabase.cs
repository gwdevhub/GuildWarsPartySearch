using GuildWarsPartySearch.Server.Options;
using Microsoft.Extensions.Options;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public class IpWhitelistConfigDatabase : IIpWhitelistDatabase
{
    private readonly IpWhitelistOptions options;

    public IpWhitelistConfigDatabase(
        IOptions<IpWhitelistOptions> options)
    {
        this.options = options.ThrowIfNull().Value.ThrowIfNull();
    }

    public Task<IEnumerable<string>> GetWhitelistedAddresses(CancellationToken cancellationToken)
    {
        return Task.FromResult(options.Addresses.AsEnumerable());
    }
}

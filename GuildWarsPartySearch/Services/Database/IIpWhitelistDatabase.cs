namespace GuildWarsPartySearch.Server.Services.Database;

public interface IIpWhitelistDatabase
{
    Task<IEnumerable<string>> GetWhitelistedAddresses(CancellationToken cancellationToken);
}

using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Services.Database;

public interface IApiKeyDatabase
{
    Task<IEnumerable<ApiKey>> GetApiKeys(CancellationToken cancellationToken);
    Task<ApiKey?> GetApiKey(string apiKey, CancellationToken cancellationToken);
    Task<bool> StoreApiKey(string apiKey, PermissionLevel permissionLevel, string description, CancellationToken cancellationToken);
    Task<bool> RecordUsage(string apiKey, CancellationToken cancellationToken);
    Task<bool> DeleteApiKey(string apiKey, CancellationToken cancellationToken);
}

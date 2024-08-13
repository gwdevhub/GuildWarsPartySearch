using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Services.Permissions;

public interface IPermissionService
{
    Task<PermissionLevel> GetPermissionLevel(string apiKey, CancellationToken cancellationToken);
    Task<bool> RecordUsage(string apiKey, CancellationToken cancellationToken);
    Task<string?> CreateApiKey(string description, PermissionLevel permissionLevel, CancellationToken cancellationToken);
    Task<bool> DeleteApiKey(string apiKey, CancellationToken cancellationToken);
}

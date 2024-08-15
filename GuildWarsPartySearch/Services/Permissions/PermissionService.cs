using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Services.Database;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Permissions;

public sealed class PermissionService : IPermissionService
{
    private readonly IApiKeyDatabase apiKeyDatabase;
    private readonly ILogger<PermissionService> logger;

    public PermissionService(
        IApiKeyDatabase apiKeyDatabase,
        ILogger<PermissionService> logger)
    {
        this.apiKeyDatabase = apiKeyDatabase.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public async Task<string?> CreateApiKey(string description, PermissionLevel permissionLevel, CancellationToken cancellationToken)
    {
        var key = GetNewApiKey();
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.CreateApiKey), string.Empty);
        scopedLogger.LogDebug($"Creating API key with permission {permissionLevel}");
        var result = await this.apiKeyDatabase.StoreApiKey(key, permissionLevel, description, cancellationToken);
        if (!result)
        {
            scopedLogger.LogError("Failed to create API key");
        }

        return result ? key : default;
    }

    public async Task<bool> DeleteApiKey(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.DeleteApiKey), string.Empty);
        scopedLogger.LogDebug($"Deleting API key");
        var result = await this.apiKeyDatabase.DeleteApiKey(apiKey, cancellationToken);
        if (!result)
        {
            scopedLogger.LogError("Failed to delete api key");
        }

        return result;
    }

    public async Task<PermissionLevel> GetPermissionLevel(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetPermissionLevel), string.Empty);
        scopedLogger.LogDebug($"Retrieving API key");
        var key = await this.apiKeyDatabase.GetApiKey(apiKey, cancellationToken);
        if (key is null)
        {
            scopedLogger.LogDebug("Failed to get api key. Returning no permissions");
            return PermissionLevel.None;
        }

        return key.PermissionLevel;
    }

    public async Task<bool> RecordUsage(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.DeleteApiKey), string.Empty);
        scopedLogger.LogDebug($"Recording API key usage");
        var result = await this.apiKeyDatabase.RecordUsage(apiKey, cancellationToken);
        if (!result)
        {
            scopedLogger.LogError("Failed to record api key usage");
        }

        return result;
    }

    private static string GetNewApiKey()
    {
        return Guid.NewGuid().ToString();
    }
}

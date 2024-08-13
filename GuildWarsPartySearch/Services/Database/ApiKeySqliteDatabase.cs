using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.Database;

public sealed class ApiKeySqliteDatabase : IApiKeyDatabase
{
    private static readonly SemaphoreSlim TableSemaphore = new(1);

    private readonly SqliteConnection connection;
    private readonly ApiWhitelistOptions options;
    private readonly ILogger<ApiKeySqliteDatabase> logger;

    public ApiKeySqliteDatabase(
        SqliteConnection connection,
        IOptions<ApiWhitelistOptions> options,
        ILogger<ApiKeySqliteDatabase> logger)
    {
        this.connection = connection.ThrowIfNull();
        this.options = options.ThrowIfNull().Value.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
        EnsureTableExists(this.connection, this.options.TableName);
    }

    public async Task<IEnumerable<ApiKey>> GetApiKeys(CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetApiKeys), string.Empty);
        try
        {
            return (await this.GetApiKeysInternal(cancellationToken));
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while retrieving api keys");
            throw;
        }
    }

    public async Task<ApiKey?> GetApiKey(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.GetApiKey), apiKey);
        try
        {
            var key = await this.GetApiKeyInternal(apiKey, cancellationToken);
            if (key is null)
            {
                scopedLogger.LogError("Failed to find API key");
                return default;
            }

            return key;
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while retrieving api keys");
            throw;
        }
    }

    public async Task<bool> StoreApiKey(string apiKey, PermissionLevel permissionLevel, string description, CancellationToken cancellationToken)
    {
        var apiKeyModel = new ApiKey
        {
            Key = apiKey,
            PermissionLevel = permissionLevel,
            Description = description,
            CreationTime = DateTime.Now,
            LastUsedTime = DateTime.Now,
            Deletable = true
        };

        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.StoreApiKey), apiKey);
        try
        {
            return await this.InsertApiKeyInternal(apiKeyModel, cancellationToken);
        }
        catch(Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while storing api key");
            throw;
        }
    }

    public async Task<bool> RecordUsage(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.RecordUsage), apiKey);
        try
        {
            return await this.UpdateLastUsedTimeInternal(apiKey, cancellationToken);
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while recording usage of key");
            throw;
        }
    }

    public async Task<bool> DeleteApiKey(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.DeleteApiKey), apiKey);
        try
        {
            return await this.DeleteApiKeyInternal(apiKey, cancellationToken);
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while deleting key");
            throw;
        }
    }

    private async Task<ApiKey?> GetApiKeyInternal(string apiKey, CancellationToken cancellationToken)
    {
        var maybeKey = this.options.Keys.FirstOrDefault(k => k.Key == apiKey);
        if (maybeKey is not null)
        {
            return new ApiKey
            {
                Key = maybeKey.Key,
                Description = maybeKey.Description,
                PermissionLevel = maybeKey.PermissionLevel,
                CreationTime = maybeKey.CreationTime,
                LastUsedTime = maybeKey.LastUsedTime,
                Deletable = false
            };
        }

        using var command = this.connection.CreateCommand();
        command.CommandText = $@"SELECT * FROM {this.options.TableName}
                                 WHERE Key == $key";
        command.Parameters.AddWithValue("$key", apiKey);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var key = new ApiKey
            {
                Key = reader.GetString(reader.GetOrdinal("Key")),
                PermissionLevel = reader.GetInt32(reader.GetOrdinal("PermissionLevel")).Cast<PermissionLevel>(),
                Description = reader.GetString(reader.GetOrdinal("Key")),
                CreationTime = reader.GetDateTime(reader.GetOrdinal("CreationTime")),
                LastUsedTime = reader.GetDateTime(reader.GetOrdinal("LastUsedTime")),
                Deletable = true
            };

            return key;
        }

        return default;
    }

    private async Task<IEnumerable<ApiKey>> GetApiKeysInternal(CancellationToken cancellationToken)
    {
        var entries = new List<ApiKey>(this.options.Keys.Select(k => new ApiKey
        {
            Key = k.Key,
            Description = k.Description,
            CreationTime = k.CreationTime,
            LastUsedTime = k.LastUsedTime,
            PermissionLevel = k.PermissionLevel,
            Deletable = false
        }));

        using var command = this.connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {this.options.TableName}";
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var apiKey = new ApiKey
            {
                Key = reader.GetString(reader.GetOrdinal("Key")),
                PermissionLevel = reader.GetInt32(reader.GetOrdinal("PermissionLevel")).Cast<PermissionLevel>(),
                Description = reader.GetString(reader.GetOrdinal("Key")),
                CreationTime = reader.GetDateTime(reader.GetOrdinal("CreationTime")),
                LastUsedTime = reader.GetDateTime(reader.GetOrdinal("LastUsedTime")),
                Deletable = true
            };

            entries.Add(apiKey);
        }

        return entries;
    }

    private async Task<bool> UpdateLastUsedTimeInternal(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.UpdateLastUsedTimeInternal), apiKey);
        try
        {
            using var command = this.connection.CreateCommand();
            command.CommandText = $@"
                        UPDATE {this.options.TableName}
                        SET LastUsedTime = CURRENT_TIMESTAMP
                        WHERE Key = $key;
                    ";
            command.Parameters.AddWithValue("key", apiKey);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            scopedLogger.LogInformation($"Updated last used time. Result {result}");
            return result == 1;
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while updating last used time");
            return false;
        }
    }

    private async Task<bool> DeleteApiKeyInternal(string apiKey, CancellationToken cancellationToken)
    {
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.DeleteApiKeyInternal), apiKey);
        try
        {
            using var command = this.connection.CreateCommand();
            command.CommandText = $@"
                        DELETE FROM {this.options.TableName}
                        WHERE Key = $key;
                    ";
            command.Parameters.AddWithValue("key", apiKey);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            scopedLogger.LogInformation($"Deleted api key. Result {result}");
            return result == 1;
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while deleting api key");
            return false;
        }
    }

    private async Task<bool> InsertApiKeyInternal(ApiKey apiKey, CancellationToken cancellationToken)
    {
        var apiKeyModel = new ApiKeyModel
        {
            Key = apiKey.Key,
            PermissionLevel = apiKey.PermissionLevel,
            CreationTime = apiKey.CreationTime,
            Description = apiKey.Description,
            LastUsedTime = apiKey.LastUsedTime
        };
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.InsertApiKeyInternal), apiKeyModel.Key ?? string.Empty);
        try
        {
            using var command = this.connection.CreateCommand();
            command.CommandText = $@"
                        INSERT INTO {this.options.TableName} (Key, PermissionLevel, Description, CreationTime, LastUsedTime)
                        VALUES ($key, $permissionLevel, $description, $creationTime, $lastUsedTime)
                    ";
            command.Parameters.AddWithValue("key", apiKeyModel.Key);
            command.Parameters.AddWithValue("permissionLevel", (int?)apiKeyModel.PermissionLevel);
            command.Parameters.AddWithValue("description", apiKeyModel.Description);
            command.Parameters.AddWithValue("creationTime", apiKeyModel.CreationTime);
            command.Parameters.AddWithValue("lastUsedTime", apiKeyModel.LastUsedTime);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            scopedLogger.LogInformation($"Inserting key. Result {result}");
            return result == 1;
        }
        catch (Exception e)
        {
            scopedLogger.LogError(e, "Encountered exception while inserting api key");
            return false;
        }
    }

    private static void EnsureTableExists(SqliteConnection connection, string tableName)
    {
        TableSemaphore.Wait();
        using var command = connection.CreateCommand();
        command.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    Key STRING PRIMARY KEY,
                    PermissionLevel INTEGER NOT NULL,
                    Description STRING NOT NULL,
                    CreationTime DATETIME NOT NULL,
                    LastUsedTime DATETIME NOT NULL
                );
            ";
        command.ExecuteNonQuery();
        TableSemaphore.Release();
    }
}

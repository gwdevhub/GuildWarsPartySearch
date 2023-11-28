using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Azure;
using Microsoft.Extensions.Options;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonTableClient<TOptions>(this IServiceCollection services)
        where TOptions : class, IAzureTableStorageOptions, new()
    {
        services.ThrowIfNull()
            .AddSingleton(sp =>
            {
                var storageOptions = sp.GetRequiredService<IOptions<StorageAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TOptions>>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TOptions>>>();
                return new NamedTableClient<TOptions>(logger, storageOptions.Value.ConnectionString!, clientOptions.Value.TableName);
            });

        return services;
    }

    public static IServiceCollection AddScopedTableClient<TOptions>(this IServiceCollection services)
        where TOptions : class, IAzureTableStorageOptions, new()
    {
        services.ThrowIfNull()
            .AddScoped(sp =>
            {
                var storageOptions = sp.GetRequiredService<IOptions<StorageAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TOptions>>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TOptions>>>();
                return new NamedTableClient<TOptions>(logger, storageOptions.Value.ConnectionString!, clientOptions.Value.TableName);
            });

        return services;
    }

    public static IServiceCollection AddSingletonBlobContainerClient<TOptions>(this IServiceCollection services)
        where TOptions : class, IAzureBlobStorageOptions, new()
    {
        services.ThrowIfNull()
            .AddSingleton(sp =>
            {
                var storageOptions = sp.GetRequiredService<IOptions<StorageAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TOptions>>();
                return new NamedBlobContainerClient<TOptions>(storageOptions.Value.ConnectionString!, clientOptions.Value.ContainerName);
            });

        return services;
    }

    public static IServiceCollection AddScopedBlobContainerClient<TOptions>(this IServiceCollection services)
        where TOptions : class, IAzureBlobStorageOptions, new()
    {
        services.ThrowIfNull()
            .AddScoped(sp =>
            {
                var storageOptions = sp.GetRequiredService<IOptions<StorageAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TOptions>>();
                return new NamedBlobContainerClient<TOptions>(storageOptions.Value.ConnectionString!, clientOptions.Value.ContainerName);
            });

        return services;
    }
}

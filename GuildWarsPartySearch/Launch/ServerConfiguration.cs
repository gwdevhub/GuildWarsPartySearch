using AspNetCoreRateLimit;
using Azure.Data.Tables;
using GuildWarsPartySearch.Common.Converters;
using GuildWarsPartySearch.Server.Endpoints;
using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.CharName;
using GuildWarsPartySearch.Server.Services.Content;
using GuildWarsPartySearch.Server.Services.Database;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.Lifetime;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Launch;

public static class ServerConfiguration
{
    public static IList<JsonConverter> SetupConverters(this IList<JsonConverter> converters)
    {
        converters.ThrowIfNull();
        converters.Add(new CampaignJsonConverter());
        converters.Add(new ContinentJsonConverter());
        converters.Add(new MapJsonConverter());
        converters.Add(new RegionJsonConverter());

        return converters;
    }

    public static WebApplicationBuilder SetupHostedServices(this WebApplicationBuilder builder)
    {
        builder.ThrowIfNull()
            .Services
            .AddHostedService<ContentRetrievalService>();

        return builder;
    }

    public static IConfigurationBuilder SetupConfiguration(this IConfigurationBuilder builder)
    {
        builder.ThrowIfNull()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Config.json");

        return builder;
    }

    public static ILoggingBuilder SetupLogging(this ILoggingBuilder builder)
    {
        builder.ThrowIfNull()
            .ClearProviders()
            .AddConsole();

        return builder;
    }

    public static WebApplicationBuilder SetupOptions(this WebApplicationBuilder builder)
    {
        builder.ThrowIfNull()
            .Services.Configure<EnvironmentOptions>(builder.Configuration.GetSection(nameof(EnvironmentOptions)))
                     .Configure<ContentOptions>(builder.Configuration.GetSection(nameof(ContentOptions)))
                     .Configure<PartySearchTableOptions>(builder.Configuration.GetSection(nameof(PartySearchTableOptions)))
                     .Configure<StorageAccountOptions>(builder.Configuration.GetSection(nameof(StorageAccountOptions)))
                     .Configure<ServerOptions>(builder.Configuration.GetSection(nameof(ServerOptions)))
                     .Configure<IpRateLimitOptions>(builder.Configuration.GetSection(nameof(IpRateLimitOptions)))
                     .Configure<IpRateLimitPolicies>(builder.Configuration.GetSection(nameof(IpRateLimitPolicies)));

        return builder;
    }

    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.ThrowIfNull();

        services.AddMemoryCache();
        services.AddInMemoryRateLimiting();
        services.AddScoped<ApiKeyProtected>();
        services.AddScoped<IServerLifetimeService, ServerLifetimeService>();
        services.AddScoped<IPartySearchDatabase, TableStorageDatabase>();
        services.AddScoped<IPartySearchService, PartySearchService>();
        services.AddScoped<ICharNameValidator, CharNameValidator>();
        services.AddSingleton<ILiveFeedService, LiveFeedService>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddScopedTableClient<PartySearchTableOptions>();
        services.AddSingletonBlobContainerClient<ContentOptions>();
        return services;
    }

    public static WebApplication SetupRoutes(this WebApplication app)
    {
        app.ThrowIfNull()
           .MapWebSocket<LiveFeed>("party-search/live-feed")
           .MapWebSocket<PostPartySearch>("party-search/update");

        return app;
    }
}

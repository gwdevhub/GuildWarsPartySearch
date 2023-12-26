using AspNetCoreRateLimit;
using GuildWarsPartySearch.Common.Converters;
using GuildWarsPartySearch.Server.Endpoints;
using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.BotStatus;
using GuildWarsPartySearch.Server.Services.CharName;
using GuildWarsPartySearch.Server.Services.Content;
using GuildWarsPartySearch.Server.Services.Database;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.Lifetime;
using GuildWarsPartySearch.Server.Services.PartySearch;
using GuildWarsPartySearch.Server.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using System.Core.Extensions;
using System.Extensions;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Launch;

public static class ServerConfiguration
{
    public static IList<JsonConverter> SetupConverters(this IList<JsonConverter> converters)
    {
        converters.ThrowIfNull();
        converters.Add(new MapJsonConverter());
        converters.Add(new ProfessionJsonConverter());

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
            .AddJsonFile("Config.json", true)
            .AddEnvironmentVariables();

        return builder;
    }

    public static ILoggingBuilder SetupLogging(this ILoggingBuilder builder)
    {
        builder.ThrowIfNull()
            .ClearProviders()
            .AddApplicationInsights()
            .AddConsole();

        return builder;
    }

    public static WebApplicationBuilder SetupOptions(this WebApplicationBuilder builder)
    {
        return builder.ThrowIfNull()
            .ConfigureAzureClientSecretCredentials<AzureCredentialsOptions>()
            .ConfigureExtended<EnvironmentOptions>()
            .ConfigureExtended<ContentOptions>()
            .ConfigureExtended<PartySearchTableOptions>()
            .ConfigureExtended<StorageAccountOptions>()
            .ConfigureExtended<ServerOptions>()
            .ConfigureExtended<IpRateLimitOptions>()
            .ConfigureExtended<IpRateLimitPolicies>()
            .ConfigureExtended<TelemetryOptions>();
    }

    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.ThrowIfNull();

        services.AddApplicationInsightsTelemetry();
        services.AddSingleton<ITelemetryInitializer, Mark4xxAsSuccessfulTelemetryInitializer>();
        services.AddApplicationInsightsTelemetryProcessor<WebSocketTelemetryProcessor>();
        services.AddMemoryCache();
        services.AddInMemoryRateLimiting();
        services.AddScoped<RequireSsl>();
        services.AddScoped<ApiKeyProtected>();
        services.AddScoped<UserAgentRequired>();
        services.AddScoped<IServerLifetimeService, ServerLifetimeService>();
        services.AddScoped<IPartySearchDatabase, TableStorageDatabase>();
        services.AddScoped<IPartySearchService, PartySearchService>();
        services.AddScoped<ICharNameValidator, CharNameValidator>();
        services.AddSingleton<ILiveFeedService, LiveFeedService>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IBotStatusService, BotStatusService>();
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
